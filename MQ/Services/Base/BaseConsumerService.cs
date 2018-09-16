using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using MQ.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MQ.Base
{
    public abstract class BaseConsumerService : BaseQueueService, IBaseConsumerService
    {
        private EventingBasicConsumer _consumer;

        public BaseConsumerService(IOptions<RabbitMqConnectionSettings> settings, ILogger<BaseConsumerService> logger)
            : base(settings, logger)
        {
            if (!Subscribe())
            {
                throw new Exception("Не удалось подключиться к брокеру сообщений");
            }
        }

        /// <summary>
        /// Читать сообщения из очереди
        /// </summary>
        /// <param name="onDequeue">Обработчик сообщения</param>
        /// <param name="onError">Обработчик ошибки</param>
        public void ProcessQueue(Action<string, ulong> onDequeue, Action<Exception, ulong> onError)
        {
            if (Channel.IsClosed)
            {
                Channel.Dispose();
                Channel = CreateChannel();
                Subscribe();
            }
            _consumer = new EventingBasicConsumer(Channel);
            _consumer.Received += OnConsumerMessageReceived;
            _consumer.Shutdown += OnConsumerShutdown;
            Channel.BasicConsume(QueueName, false, _consumer);

            void OnConsumerMessageReceived(object o, BasicDeliverEventArgs e)
            {
                var queuedMessage = Encoding.UTF8.GetString(e.Body);
                onDequeue.Invoke(queuedMessage, e.DeliveryTag);
            }

            void OnConsumerShutdown(object o, ShutdownEventArgs e)
            {
                ProcessQueue(onDequeue, onError);
            }
        }

        public void MarkAsProcessed(ulong deliveryTag)
        {
            Channel.BasicAck(deliveryTag, false);
        }

        public void MarkAsCancelled(ulong deliveryTag)
        {
            Channel.BasicNack(deliveryTag, false, false);
        }

        protected override IModel CreateChannel()
        {
            base.CreateChannel();

            Channel.BasicQos(0, 1, false);

            return Channel;
        }

        /// <summary>
        /// Подписаться на очередь
        /// </summary>
        protected sealed override bool Subscribe()
        {
            if (!base.Subscribe())
            {
                return false;
            }
            Channel.QueueBind(QueueName, ExchangeName, RoutingKeyName);

            return true;
        }

        /// <summary>
        /// Отписаться от очереди
        /// </summary>
        protected sealed override void Unsubscribe()
        {
            if (!String.IsNullOrEmpty(_consumer?.ConsumerTag))
            {
                Channel?.BasicCancel(_consumer?.ConsumerTag);
            }
            base.Unsubscribe();
        }
    }
}
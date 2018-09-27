using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Base;
using MQ.Configuration.Base;
using MQ.Messages;
using MQ.PersistentConnection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MQ.Services.Base
{
    public abstract class ConsumerService<TDocument> : BaseQueueService, IConsumerService
        where TDocument : Document
    {
        protected readonly IModel Channel;

        protected ConsumerService(
            IPersistentConnection persistentConnection,
            IOptions<BaseQueueSettings> settings,
            ILogger<ConsumerService<TDocument>> logger)
            : base(persistentConnection, settings, logger)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            Channel = CreateChannel();
        }

        public void MarkAsProcessed(ulong deliveryTag)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            Channel.BasicAck(deliveryTag, false);
        }

        public void ReEnqueue(ulong deliveryTag)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            Channel.BasicNack(deliveryTag, false, true);
        }

        public void MarkAsCancelled(ulong deliveryTag)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            Channel.BasicReject(deliveryTag, false);
        }

        /// <summary>
        /// Читать сообщения из очереди
        /// </summary>
        /// <param name="onDequeue">Обработчик сообщения</param>
        /// <param name="onError">Обработчик ошибки</param>
        public void ProcessQueue<T>(Func<T, ulong, bool> onDequeue, Action<Exception, ulong> onError)
            where T : EventMessage
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (sender, ea) =>
            {
                var parsedMessage = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(ea.Body));
                var processingSucceded = onDequeue.Invoke(parsedMessage, ea.DeliveryTag);
                try
                {
                    if (processingSucceded)
                    {
                        MarkAsProcessed(ea.DeliveryTag);
                    }
                    else
                    {
                        InternalReEnqueue(ea);
                    }
                }
                catch (Exception e)
                {
                    onError.Invoke(e, ea.DeliveryTag);
                    Logger.LogError(e, "Ошибка во время отправки результата обработки сообщения");
                }
            };
            consumer.Shutdown += (sender, ea) => ProcessQueue(onDequeue, onError);

            Channel.BasicConsume(Settings.QueueName, false, consumer);
        }

        public void ProcessQueue<T>(Func<T, ulong, Task<bool>> onDequeue, Action<Exception, ulong> onError)
            where T : EventMessage
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += async (sender, ea) =>
            {
                var str = Encoding.UTF8.GetString(ea.Body);
                var parsedMessage = JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                var processingSucceded = await onDequeue.Invoke(parsedMessage, ea.DeliveryTag);
                try
                {
                    if (processingSucceded)
                    {
                        MarkAsProcessed(ea.DeliveryTag);
                    }
                    else
                    {
                        InternalReEnqueue(ea);
                    }
                }
                catch (Exception e)
                {
                    onError.Invoke(e, ea.DeliveryTag);
                    Logger.LogError(e, "Ошибка во время отправки результата обработки сообщения");
                }
            };

            consumer.Shutdown += (sender, ea) => ProcessQueue(onDequeue, onError);

            Channel.BasicConsume(Settings.QueueName, false, consumer);
        }

        protected sealed override IModel CreateChannel()
        {
            var channel = base.CreateChannel();
            channel.BasicQos(0, 1, false);

            return channel;
        }

        protected void InternalReEnqueue(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            var retryCount = GetRetryCount(basicDeliverEventArgs.BasicProperties);

            if (retryCount > 0)
            {
                SetRetryProperties(basicDeliverEventArgs.BasicProperties, --retryCount);

                Channel.BasicPublish(basicDeliverEventArgs.Exchange, basicDeliverEventArgs.RoutingKey,
                    basicDeliverEventArgs.BasicProperties, basicDeliverEventArgs.Body);
                Channel.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
            }
            else
            {
                Channel.BasicNack(basicDeliverEventArgs.DeliveryTag, false, false);
            }

            int GetRetryCount(IBasicProperties properties)
            {
                return (int?) properties.Headers?["Retries"] ?? Settings.MaxRetryCount;
            }

            void SetRetryProperties(IBasicProperties properties, int retryCounts)
            {
                var newDelay = Settings.RetryDelay * (Settings.MaxRetryCount - retryCount) * 1000;
                properties.Headers = properties.Headers ?? new Dictionary<string, object>();
                properties.Headers["Retries"] = retryCounts;
                properties.Headers["x-delay"] = newDelay;
            }
        }
    }
}
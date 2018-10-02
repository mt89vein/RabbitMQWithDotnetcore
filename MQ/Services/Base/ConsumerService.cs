using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Base;
using MQ.Configuration.Base;
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

        public void ProcessQueue<T>(Func<T, Task<bool>> onDequeue, Action<Exception, T> onError)
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
                var processingSucceded = await onDequeue.Invoke(parsedMessage);
                try
                {
                    if (processingSucceded)
                    {
                        MarkAsProcessed(ea.DeliveryTag);
                    }
                    else
                    {
                        ReEnqueue(ea);
                    }
                }
                catch (Exception e)
                {
                    onError.Invoke(e, parsedMessage);
                    Logger.LogError(e, "Ошибка во время отправки результата обработки сообщения");
                }
            };

            consumer.Shutdown += (sender, ea) => ProcessQueue(onDequeue, onError);

            Channel.BasicConsume(Settings.QueueName, false, consumer);
        }

        protected void MarkAsProcessed(ulong deliveryTag)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }
            Logger.LogDebug($"Mark as processed - {deliveryTag}");
            Channel.BasicAck(deliveryTag, false);
        }

        protected sealed override IModel CreateChannel()
        {
            var channel = base.CreateChannel();
            channel.BasicQos(0, 1, false);

            return channel;
        }

        protected void ReEnqueue(BasicDeliverEventArgs basicDeliverEventArgs)
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
                Logger.LogDebug($"Retry, {basicDeliverEventArgs.Body} - {retryCount}");
            }
            else
            {
                Channel.BasicNack(basicDeliverEventArgs.DeliveryTag, false, false);
                Logger.LogDebug($"BasicNack {basicDeliverEventArgs.Body}");
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
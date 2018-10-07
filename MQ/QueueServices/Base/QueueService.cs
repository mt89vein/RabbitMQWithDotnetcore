using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Base;
using MQ.Configuration.Base;
using MQ.PersistentConnection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace MQ.Services
{
    public class QueueService : IEventBus, IDisposable
    {
        private readonly IBasicProperties _basicProperties;
        private readonly ILogger<QueueService> _logger;
        private readonly IPersistentConnection _persistentConnection;
        protected readonly BaseQueueSettings Settings;
        private IModel _consumerChannel;
        private IModel _producerChannel;

        protected QueueService(IPersistentConnection persistentConnection,
            IOptions<BaseQueueSettings> settings, ILogger<QueueService> logger)
        {
            Settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger =
                logger ?? throw new ArgumentNullException(nameof(logger));
            _persistentConnection =
                persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _consumerChannel = CreateConsumerChannel();
            _producerChannel = CreateProducerChannel();
            _basicProperties = new BasicProperties
            {
                Persistent = true,
                DeliveryMode = 2,
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8.EncodingName
            };
        }

        public void Dispose()
        {
            _consumerChannel?.Dispose();
            _producerChannel?.Dispose();
            GC.SuppressFinalize(this);
        }

        public void PublishMessage<T>(T eventMessage) where T : EventMessage
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
            if (_producerChannel == null)
            {
                _producerChannel = CreateProducerChannel();
            }

            var eventName = eventMessage.GetType().Name;

            _producerChannel.BasicPublish(Settings.ExchangeName, eventName, _basicProperties,
                GetConvertedMessage());

            byte[] GetConvertedMessage()
            {
                if (eventMessage == null)
                {
                    throw new ArgumentNullException(nameof(eventMessage));
                }
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventMessage, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }));
            }
        }

        public void ProcessQueue<T>(Func<T, Task<bool>> onDequeue, Action<Exception, T> onError) where T : EventMessage
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
            }

            var consumer = new EventingBasicConsumer(_consumerChannel);
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
                    _logger.LogError(e, "Ошибка во время отправки результата обработки сообщения");
                }
            };

            consumer.Shutdown += (sender, ea) => ProcessQueue(onDequeue, onError);
            _consumerChannel.BasicConsume(Settings.QueueName, false, consumer);
        }

        private IModel CreateConsumerChannel()
        {
            var channel = CreateBasicChannel();
            channel.BasicQos(0, 1, false);

            channel.QueueDeclare(
                Settings.QueueName,
                true,
                false,
                false
            );

            channel.QueueBind(
                Settings.QueueName,
                Settings.ExchangeName,
                Settings.RoutingKey);

            return channel;
        }

        private IModel CreateProducerChannel()
        {
            var channel = CreateBasicChannel();
            channel.ConfirmSelect();
            return channel;
        }

        private IModel CreateBasicChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(
                Settings.ExchangeName,
                Settings.ExchangeType,
                true,
                false,
                Settings.WithDelay
                    ? new Dictionary<string, object>
                    {
                        {"x-delayed-type", "direct"}
                    }
                    : null
            );

            return channel;
        }

        private void MarkAsProcessed(ulong deliveryTag)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
            _logger.LogDebug($"Mark as processed - {deliveryTag}");
            _consumerChannel.BasicAck(deliveryTag, false);
        }

        private void ReEnqueue(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var retryCount = GetRetryCount(basicDeliverEventArgs.BasicProperties);

            if (retryCount > 0)
            {
                SetRetryProperties(basicDeliverEventArgs.BasicProperties, --retryCount);

                _consumerChannel.BasicPublish(basicDeliverEventArgs.Exchange, basicDeliverEventArgs.RoutingKey,
                    basicDeliverEventArgs.BasicProperties, basicDeliverEventArgs.Body);
                _consumerChannel.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
                _logger.LogDebug($"Retry, {basicDeliverEventArgs.Body} - {retryCount}");
            }
            else
            {
                _consumerChannel.BasicNack(basicDeliverEventArgs.DeliveryTag, false, false);
                _logger.LogDebug($"BasicNack {basicDeliverEventArgs.Body}");
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
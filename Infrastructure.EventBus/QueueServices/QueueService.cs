using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.EventBus.Abstractions;
using Infrastructure.EventBus.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing;

namespace Infrastructure.EventBus.QueueServices
{
    public class QueueService : IEventBus, IDisposable
    {
        private readonly IBasicProperties _basicProperties;
        private readonly ILogger<QueueService> _logger;
        private readonly IPersistentConnection _persistentConnection;
        protected readonly QueueSettings Settings;
        private IModel _consumerChannel;
        private IModel _producerChannel;

        protected QueueService(IPersistentConnection persistentConnection,
            IOptions<QueueSettings> settings, ILogger<QueueService> logger)
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
            if (_consumerChannel?.IsClosed == false)
            {
                _consumerChannel.QueueUnbind(
                    Settings.QueueName,
                    Settings.ExchangeName,
                    Settings.RoutingKey);
                _consumerChannel.Close();
                _consumerChannel.Dispose();

            }
            if (_producerChannel?.IsClosed == false)
            {
                _producerChannel.QueueUnbind(
                    Settings.QueueName,
                    Settings.ExchangeName,
                    Settings.RoutingKey);
                _producerChannel.Close();
                _producerChannel.Dispose();
            }
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

        public void ProcessQueue<T>(Func<T, CancellationToken, Task<bool>> onDequeue, Action<Exception, T> onError, CancellationToken cancellationToken) where T : EventMessage
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
                    TypeNameHandling = TypeNameHandling.All
                });

                try
                {
                    var processingSucceded = await onDequeue.Invoke(parsedMessage, cancellationToken);
                    if (processingSucceded)
                    {
                        MarkAsProcessed(ea.DeliveryTag);
                    }
                    else
                    {
                        ReEnqueue(ea);
                    }
                }
                catch (OperationCanceledException)
                {
                    Dispose();
                }
                catch (Exception e)
                {
                    onError.Invoke(e, parsedMessage);
                    _logger.LogError(e, "Ошибка во время отправки результата обработки сообщения");
                }
            };

            _consumerChannel.BasicConsume(Settings.QueueName, false, consumer);
            consumer.Shutdown += (sender, ea) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    ProcessQueue(onDequeue, onError, cancellationToken);
                }
            };
        }

        private IModel CreateConsumerChannel()
        {
            var channel = CreateBasicChannel();

            channel.CallbackException += (sender, args) => _consumerChannel = CreateConsumerChannel();

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
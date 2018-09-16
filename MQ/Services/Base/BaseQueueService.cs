using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace MQ.Base
{
    public abstract class BaseQueueService : IDisposable
    {
        protected readonly ILogger<BaseQueueService> Logger;

        private readonly RabbitMqConnectionSettings _settings;

        private readonly Lazy<IConnection> _connection;

        protected BaseQueueService(IOptions<RabbitMqConnectionSettings> settings, ILogger<BaseQueueService> logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            _settings = settings.Value;
            Logger = logger ?? throw  new ArgumentNullException(nameof(logger));
            _connection = new Lazy<IConnection>(Connect);
        }

        /// <summary>
        /// Канал для доступа к методам работы с брокером сообщений
        /// </summary>
        protected IModel Channel { get; set; }

        /// <summary>
        /// Соединение с брокером сообщений
        /// </summary>
        /// <value>The connection to rabbit</value>
        protected IConnection Connection => _connection.Value;

        /// <summary>
        /// Название очереди
        /// </summary>
        protected string QueueName => _settings.QueueName;

        /// <summary>
        /// Название названия обменника сообщений
        /// </summary>
        protected string ExchangeName => _settings.ExchangeName;

        /// <summary>
        /// Тип обменника
        /// </summary>
        protected string ExchangeType => _settings.ExchangeType;

        /// <summary>
        /// Ключ роутинга
        /// </summary>
        protected string RoutingKeyName => _settings.RoutingKey;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Подключиться к сервису сообщений
        /// </summary>
        /// <returns></returns>
        protected IConnection Connect()
        {
            var retryAttempt = 0;
            var maxAttemtps = _settings.RetryConnectionAttempt ?? 5;
            while (retryAttempt < maxAttemtps)
            {
                retryAttempt++;

                try
                {
                    var connectionFactory = new ConnectionFactory
                    {
                        HostName = _settings.HostName,
                        Port = _settings.Port ?? 5672,
                        //UserName = _settings.UserName,
                        //Password = _settings.Password,
                        RequestedHeartbeat = _settings.RequestedHeartbeat,
                        NetworkRecoveryInterval = _settings.NetworkRecoveryInterval.HasValue
                            ? TimeSpan.FromSeconds(_settings.NetworkRecoveryInterval.Value)
                            : TimeSpan.FromSeconds(10)
                    };

                    return connectionFactory.CreateConnection();
                }
                catch (EndOfStreamException ex)
                {
                    Logger.LogError(ex, "Ошибка при попытке подключиться к брокеру сообщений");
                }
                catch (BrokerUnreachableException ex)
                {
                    Logger.LogError(ex, "Брокер сообщений недоступен");
                }

                Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }

            throw new Exception("Не удалось соединиться с брокером сообщений");
        }

        protected virtual IModel CreateChannel()
        {
            Channel = Connection.CreateModel();

            if (!string.IsNullOrWhiteSpace(ExchangeName))
            {
                Channel.ExchangeDeclare(ExchangeName, ExchangeType);
            }

            Channel.QueueDeclare(QueueName, true, false, false);

            return Channel;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unsubscribe();
                Channel?.Close();
                Channel?.Dispose();
                Connection?.Close();
                Connection?.Dispose();
            }
        }

        /// <summary>
        /// Базовая подписка, подключиться и создать канал
        /// </summary>
        protected virtual bool Subscribe()
        {
            try
            {
                Channel = CreateChannel();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Ошибка при попытке подписаться создания канала");
                return false;
            }
        }

        /// <summary>
        /// Отписаться от канала
        /// </summary>
        protected virtual void Unsubscribe()
        {
            if (Channel == null || Channel.IsClosed)
            {
                return;
            }

            Channel?.QueueUnbind(QueueName, ExchangeName, RoutingKeyName);
            Channel?.Close();
        }
    }
}
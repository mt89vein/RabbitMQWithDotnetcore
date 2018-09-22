using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using MQ.PersistentConnection;
using RabbitMQ.Client;

namespace MQ.Base
{
    public abstract class BaseQueueService : IDisposable
    {
        protected readonly ILogger<BaseQueueService> Logger;
        protected readonly IPersistentConnection PersistentConnection;
        protected readonly BaseQueueSettings Settings;

        protected BaseQueueService(IPersistentConnection persistentConnection,
            IOptions<BaseQueueSettings> settings, ILogger<BaseQueueService> logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            Settings = settings.Value;
            Logger =
                logger ?? throw new ArgumentNullException(nameof(logger));
            PersistentConnection =
                persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        protected virtual IModel CreateChannel()
        {
            var channel = PersistentConnection.CreateModel();

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
    }
}
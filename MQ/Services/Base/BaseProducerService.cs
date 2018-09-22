using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.Interfaces.Messages;
using MQ.PersistentConnection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace MQ.Base
{
    public abstract class BaseProducerService : BaseQueueService, IBaseProducerService
    {
        private readonly IBasicProperties _properties;
        protected readonly IModel Channel;

        protected BaseProducerService(
            IPersistentConnection persistentConnection,
            IOptions<BaseQueueSettings> settings,
            ILogger<BaseProducerService> logger)
            : base(persistentConnection, settings, logger)
        {
            _properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8.EncodingName
            };
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            Channel = CreateChannel();
        }

        protected sealed override IModel CreateChannel()
        {
            var channel = base.CreateChannel();
            channel.ConfirmSelect();
            return channel;
        }

        public ulong PublishMessage(IMessage message)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            var nextDeliveryTag = Channel.NextPublishSeqNo;
            Channel.BasicPublish(Settings.ExchangeName, Settings.RoutingKey, _properties, GetConvertedMessage());

            return nextDeliveryTag;

            byte[] GetConvertedMessage()
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            }
        }

        public void RemoveMessage(ulong deliveryTag)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            Channel.BasicReject(deliveryTag, false);
        }
    }
}
using System;
using System.Text;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Base;
using MQ.Configuration.Base;
using MQ.PersistentConnection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace MQ.Services.Base
{
    public abstract class ProducerService<TDocument> : BaseQueueService, IProducerService
        where TDocument : Document
    {
        private readonly IBasicProperties _properties;
        protected readonly IModel Channel;

        protected ProducerService(
            IPersistentConnection persistentConnection,
            IOptions<BaseQueueSettings> settings,
            ILogger<ProducerService<TDocument>> logger)
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

        public ulong PublishMessage(EventMessage eventMessage)
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

        public void RemoveMessage(ulong deliveryTag)
        {
            if (!PersistentConnection.IsConnected)
            {
                PersistentConnection.TryConnect();
            }

            Channel.BasicReject(deliveryTag, false);
        }

        protected sealed override IModel CreateChannel()
        {
            var channel = base.CreateChannel();
            channel.ConfirmSelect();
            return channel;
        }
    }
}
using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.Interfaces.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MQ.Base
{
    public abstract class BaseProducerService : BaseQueueService, IBaseProducerService
    {
        private IBasicProperties _properties;

        public BaseProducerService(IOptions<RabbitMqConnectionSettings> settings, ILogger<BaseProducerService> logger)
            : base(settings, logger)
        {
            if (!Subscribe())
            {
                throw new Exception("Не удалось подключиться к брокеру сообщений");
            }
        }

        public ulong PublishMessage(IMessage message)
        {
            Channel.BasicPublish(ExchangeName, RoutingKeyName, _properties, GetConvertedMessage());
            
            return Channel.NextPublishSeqNo;

            byte[] GetConvertedMessage()
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            }
        }

        public void RemoveMessage(ulong id)
        {
            Channel.BasicReject(id, false);
        }

        /// <summary>
        /// Событие возникает если сообщение не было доставлено (ошибки роутинга)
        /// </summary>
        private void Channel_BasicReturn(object sender, BasicReturnEventArgs e)
        {
            Logger.LogError($"Недоставлено сообщение в брокер сообщений: {e.ReplyCode} {e.ReplyText}");
        }

        /// <summary>
        /// Событие возникает в случае отмены сообщения
        /// </summary>
        private void Channel_BasicNacks(object sender, BasicNackEventArgs e)
        {
            Logger.LogError($"Сообщение было отменено: {e.DeliveryTag}");
        }

        private void Channel_BasicAcks(object sender, BasicAckEventArgs e)
        {
            Logger.LogInformation($"Принято сообщение в брокере сообщений: {e.DeliveryTag}");
        }

        protected override IModel CreateChannel()
        {
            base.CreateChannel();

            Channel.ConfirmSelect();
            Channel.WaitForConfirmsOrDie();

            Channel.BasicAcks += Channel_BasicAcks;
            Channel.BasicNacks += Channel_BasicNacks;
            Channel.BasicReturn += Channel_BasicReturn;
            Channel.ModelShutdown += (sender, args) => Subscribe();

            return Channel;
        }

        protected sealed override bool Subscribe()
        {
            if (!base.Subscribe())
            {
                return false;
            }

            _properties = Channel.CreateBasicProperties();
            _properties.Persistent = true;
            _properties.ContentType = "application/json";
            _properties.ContentEncoding = Encoding.UTF8.EncodingName;

            return true;
        }
    }
}
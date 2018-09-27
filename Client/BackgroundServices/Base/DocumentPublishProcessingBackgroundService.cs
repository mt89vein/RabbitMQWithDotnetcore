using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Base;
using MQ.Abstractions.Messages;
using MQ.Configuration.Base;

namespace Client
{
    /// <summary>
    /// Воркер фоновых задач для первичной публикации документа
    /// В случае, если документ не будет опубликован (в случае возникновения внештатных ситуаций), он будет возвращен в конец
    /// очереди и увеличен счетчик повторных попыток.
    /// В случае, если вернут Processing, то документ уйдет в очередь для обновления статуса
    /// В случае, если вернут Success или Failed, то будет отправлено событие о завершении
    /// </summary>
    public abstract class DocumentPublishProcessingBackgroundService<
        TDocumentPublishConsumerService,
        TDocumentPublishUpdateProducerService,
        TDocumentPublishConsumerServiceSettings,
        TPublishMessage,
        TPublishUpdateMessage> : BackgroundService
        where TDocumentPublishConsumerService : class, IConsumerService
        where TDocumentPublishUpdateProducerService : class, IProducerService
        where TDocumentPublishConsumerServiceSettings : DocumentPublishQueueSettings, new()
        where TPublishMessage : DocumentPublishEventMessage
        where TPublishUpdateMessage : DocumentPublishUpdateEventMessage, new()
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TDocumentPublishConsumerServiceSettings _settings;

        protected readonly ILogger<DocumentPublishProcessingBackgroundService<TDocumentPublishConsumerService,
            TDocumentPublishUpdateProducerService, TDocumentPublishConsumerServiceSettings, TPublishMessage,
            TPublishUpdateMessage>> Logger;

        protected DocumentPublishProcessingBackgroundService(
            IOptions<TDocumentPublishConsumerServiceSettings> settings,
            ILogger<DocumentPublishProcessingBackgroundService<TDocumentPublishConsumerService,
                TDocumentPublishUpdateProducerService, TDocumentPublishConsumerServiceSettings, TPublishMessage,
                TPublishUpdateMessage>> logger,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < _settings.ConsumersCount; i++)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var publishConsumerService = scope.ServiceProvider
                            .GetRequiredService<TDocumentPublishConsumerService>();
                        var publishUpdateProducerService = scope.ServiceProvider
                            .GetRequiredService<TDocumentPublishUpdateProducerService>();
                        var consumerThread = new Thread(() => MakeConsumer(publishConsumerService,
                            publishUpdateProducerService, cancellationToken))
                        {
                            IsBackground = true
                        };
                        consumerThread.Start();
                    }
                }
            }, cancellationToken);
        }

        private void MakeConsumer(TDocumentPublishConsumerService documentPublishConsumerService,
            TDocumentPublishUpdateProducerService documentPublishUpdateProducerService,
            CancellationToken cancellationToken)
        {
            documentPublishConsumerService.ProcessQueue<TPublishMessage>(async (message, deliveryTag) =>
            {
                try
                {
                    var documentPublicationInfo = await ProcessMessage(message, cancellationToken);

                    if (documentPublicationInfo.ResultType == PublicationResultType.Processing)
                    {
                        PublishUpdateMessage(documentPublishUpdateProducerService, message,
                            documentPublicationInfo.RefId);
                    }

                    SavePublishResult(documentPublicationInfo);
                    SendNotification(documentPublicationInfo);

                    return true;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Возникла ошибка при обработке сообщения");
                    return false;
                }
            }, RaiseException);
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>must return http response eventMessage</returns>
        protected abstract Task<DocumentPublicationInfo> ProcessMessage(TPublishMessage message,
            CancellationToken cancellationToken);

        /// <summary>
        /// Поставить на очередь на Update
        /// </summary>
        /// <param name="documentPublishUpdateProducerService"></param>
        /// <param name="message">Сообщение с данными для апдейта</param>
        /// <param name="refId">Идентификатор по которому можно запросить результат публикации</param>
        /// <returns></returns>
        protected virtual void PublishUpdateMessage(
            TDocumentPublishUpdateProducerService documentPublishUpdateProducerService, TPublishMessage message,
            string refId)
        {
            var updateMessage = new TPublishUpdateMessage
            {
                UserId = message.UserId,
                DocumentType = message.DocumentType,
                RevisionIdentity = message.RevisionIdentity,
                UserInputData = message.UserInputData,
                RefId = refId,
                TimeStamp = DateTime.Now
            };
            documentPublishUpdateProducerService.PublishMessage(updateMessage);
        }

        /// <summary>
        /// Default Error handler..
        /// </summary>
        /// <param name="ex">Пойманная ошибка</param>
        /// <param name="deliveryTag">Идентификатор сообщения</param>
        protected virtual void RaiseException(Exception ex, ulong deliveryTag)
        {
            Logger.LogError(ex, "RaiseException" + deliveryTag);
        }

        /// <summary>
        /// Отправить уведомление о состоянии документа
        /// </summary>
        /// <param name="documentPublicationInfo">Информация о публикации документа</param>
        protected abstract void SendNotification(DocumentPublicationInfo documentPublicationInfo);

        /// <summary>
        /// Сохранить информацию о публикации документа
        /// </summary>
        /// <param name="documentPublicationInfo">Информация о публикации документа</param>
        protected abstract void SavePublishResult(DocumentPublicationInfo documentPublicationInfo);
    }
}
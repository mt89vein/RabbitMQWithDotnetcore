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
    public abstract class DocumentPublishUpdateBackgroundService<
        TDocumentPublishUpdateConsumerService,
        TDocumentPublishUpdateConsumerServiceSettings,
        TPublishUpdateMessage> : BackgroundService
        where TDocumentPublishUpdateConsumerService : class, IConsumerService
        where TDocumentPublishUpdateConsumerServiceSettings : DocumentPublishUpdateQueueSettings, new()
        where TPublishUpdateMessage : DocumentPublishUpdateEventMessage
    {
        private readonly IServiceProvider _provider;
        private readonly DocumentPublishUpdateQueueSettings _settings;

        protected readonly ILogger<DocumentPublishUpdateBackgroundService<TDocumentPublishUpdateConsumerService,
            TDocumentPublishUpdateConsumerServiceSettings, TPublishUpdateMessage>> Logger;

        protected DocumentPublishUpdateBackgroundService(
            IServiceProvider provider,
            IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService<TDocumentPublishUpdateConsumerService,
                TDocumentPublishUpdateConsumerServiceSettings, TPublishUpdateMessage>> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < _settings.ConsumersCount; i++)
                {
                    using (var scope = _provider.CreateScope())
                    {
                        var documentPublishUpdateProcessingService = scope.ServiceProvider
                            .GetRequiredService<TDocumentPublishUpdateConsumerService>();
                        var consumerThread =
                            new Thread(() => MakeConsumer(documentPublishUpdateProcessingService, cancellationToken))
                            {
                                IsBackground = true
                            };
                        consumerThread.Start();
                    }
                }
            }, cancellationToken);
        }

        private void MakeConsumer(TDocumentPublishUpdateConsumerService documentPublishUpdateConsumerService,
            CancellationToken cancellationToken)
        {
            documentPublishUpdateConsumerService.ProcessQueue<TPublishUpdateMessage>(async (message, deliveryTag) =>
            {
                try
                {
                    var documentPublicationInfo = await ProcessMessage(message, cancellationToken);

                    var processingFinished = documentPublicationInfo.ResultType != PublicationResultType.Processing;

                    if (processingFinished)
                    {
                        SavePublishResult(documentPublicationInfo);
                        SendNotification(documentPublicationInfo);
                    }

                    return processingFinished;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Возникла ошибка при обработке сообщения");
                    return false;
                }

                /*
                 *  if Error occured.. and need to republish (Внештатные ситуации - Timeout сети и т.д) 
                 *    _publishService.DocumentOnePublishUpdateConsumerService.MarkAsProcessed(deliveryTag);
                 *     if (Settings.MaxRetryCount > msg.RetryCount) {
                 *          msg.RetryCount ++
                 *          _publishService.DocumentOnePublishUpdateProducerService.PublishMessage(msg);
                 *          return;
                 *     }
                 *      
                 *   Если опять вернула processing... положить в очередь в конец и взять другое
                 *   [Рассмотреть случай, когда всего один-два документа в очереди и они туда сюда ходят.. добавить Delay между ретраями]
                 *   
                 *   Если вернула error или success, то обновить данные, изменить статус документа
                 */
            }, RaiseException);
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>must return http response message</returns>
        protected abstract Task<DocumentPublicationInfo> ProcessMessage(TPublishUpdateMessage message,
            CancellationToken cancellationToken);

        protected void RaiseException(Exception ex, ulong deliveryTag)
        {
            Logger.LogError(ex, "RaiseException" + deliveryTag);
        }

        protected virtual void SendNotification(DocumentPublicationInfo documentPublicationInfo)
        {
            Logger.LogInformation($"document update notification, refId is: {documentPublicationInfo.RefId}");
        }

        protected virtual void SavePublishResult(DocumentPublicationInfo documentPublicationInfo)
        {
            Logger.LogInformation($"document update save result, refId is: {documentPublicationInfo.RefId}");
        }
    }
}
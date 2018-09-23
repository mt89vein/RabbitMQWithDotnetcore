using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.Messages;
using Newtonsoft.Json;

namespace Client
{
    internal class DocumentPublishUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly DocumentPublishUpdateQueueSettings _settings;
        private readonly IServiceProvider _provider;

        public DocumentPublishUpdateBackgroundService(
            IServiceProvider provider,
            IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                        var documentPublishUpdateProcessingService = scope.ServiceProvider.GetRequiredService<IDocumentPublishUpdateProcessingService>();
                        var consumerThread = new Thread(() => MakeConsumer(documentPublishUpdateProcessingService, cancellationToken))
                        {
                            IsBackground = true
                        };
                        consumerThread.Start();
                    }
                }
            }, cancellationToken);
        }

        private void MakeConsumer(IDocumentPublishUpdateProcessingService documentPublishUpdateProcessingService, CancellationToken cancellationToken)
        {
            documentPublishUpdateProcessingService.ProcessQueue(async (message, deliveryTag) =>
            {
                try
                {
                    var publishUpdateQueueMessage = JsonConvert.DeserializeObject<PublishUpdateQueueMessage>(message);
                    var documentPublicationInfo = await ProcessMessage(publishUpdateQueueMessage, cancellationToken);

                    return documentPublicationInfo.ResultType != PublicationResultType.Processing;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Возникла ошибка при обработке сообщения");
                    return false;
                }

                /*
                 *  if Error occured.. and need to republish (Внештатные ситуации - Timeout сети и т.д) 
                 *    _publishService.DocumentPublishUpdateProcessingService.MarkAsProcessed(deliveryTag);
                 *     if (Settings.MaxRetryCount > msg.RetryCount) {
                 *          msg.RetryCount ++
                 *          _publishService.DocumentPublishUpdateService.PublishMessage(msg);
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
        private static async Task<DocumentPublicationInfo> ProcessMessage(PublishUpdateQueueMessage message, CancellationToken cancellationToken)
        {
            await Task.Delay(0, cancellationToken);

            int? loadId = null;
            var result =  (PublicationResultType)new Random().Next(0, 3);
            if (result == PublicationResultType.Success)
            {
                loadId = new Random().Next(1, int.MaxValue);
            }

            return new DocumentPublicationInfo(message.RefId, result, loadId);
        }

        private void RaiseException(Exception ex, ulong deliveryTag)
        {
            _logger.LogError(ex, "RaiseException" + deliveryTag);
        }
    }
}
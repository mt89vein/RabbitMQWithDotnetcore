using System;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly DocumentPublishUpdateQueueSettings _settings;

        public DocumentPublishUpdateBackgroundService(IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < _settings.ConsumersCount; i++)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var documentPublishUpdateProcessingService = scope.ServiceProvider
                            .GetRequiredService<IDocumentPublishUpdateProcessingService>();
                        var consumerThread = new Thread(() => MakeConsumer(documentPublishUpdateProcessingService, stoppingToken))
                        {
                            IsBackground = true
                        };
                        consumerThread.Start();
                    }
                }
            }, stoppingToken);
        }

        private void MakeConsumer(IDocumentPublishUpdateProcessingService documentPublishUpdateProcessingService, CancellationToken cancellationToken)
        {
            documentPublishUpdateProcessingService.ProcessQueue(async (message, deliveryTag) =>
            {
                var msg = DeserializeMessage(message);

                // отправка запроса на обновление
                await ProcessMessage(msg, cancellationToken);

                /*
                 *  if Error occured.. and need to republish (Внештатные ситуации - Timeout сети и т.д) 
                 *    _publishService.DocumentPublishUpdateProcessingService.MarkAsProcessed(deliveryTag);
                 *     if (_settings.MaxRetryCount > msg.RetryCount) {
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
                documentPublishUpdateProcessingService.MarkAsProcessed(deliveryTag);
            }, RaiseException);
        }

        private static PublishUpdateQueueMessage DeserializeMessage(string message)
        {
            return JsonConvert.DeserializeObject<PublishUpdateQueueMessage>(message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stoppingToken"></param>
        /// <returns>must return http response message</returns>
        private static async Task ProcessMessage(PublishUpdateQueueMessage message, CancellationToken stoppingToken)
        {
            await Task.Delay(0, stoppingToken);
        }

        private void RaiseException(Exception ex, ulong deliveryTag)
        {
            _logger.LogError(ex, "RaiseException" + deliveryTag);
        }
    }
}
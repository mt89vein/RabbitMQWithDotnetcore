using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using MQ.Messages;
using MQ.Services.AggregatorService;
using Newtonsoft.Json;

namespace Client
{
    internal class DocumentPublishUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IPublishService _publishService;
        private readonly DocumentPublishUpdateQueueSettings _settings;

        public DocumentPublishUpdateBackgroundService(IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService> logger,
            IPublishService publishService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishService = publishService ?? throw new ArgumentNullException(nameof(publishService));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < _settings.ConsumersCount; i++)
                {
                    var consumerThread = new Thread(() => StartConsumer(stoppingToken));
                    consumerThread.Start();
                }
            }, stoppingToken);
        }

        private void StartConsumer(CancellationToken stoppingToken)
        {
            _publishService.DocumentPublishUpdateProcessingService.ProcessQueue(async (message, deliveryTag) =>
            {
                var msg = DeserializeMessage(message);

                // отправка запроса на обновление
                await ProcessMessage(msg, stoppingToken);

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
                _publishService.DocumentPublishUpdateProcessingService.MarkAsProcessed(deliveryTag);
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
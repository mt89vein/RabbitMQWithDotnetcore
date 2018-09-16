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
    /// <summary>
    /// Воркер фоновых задач для первичной публикации документа
    /// В случае, если документ не будет опубликован (в случае возникновения внештатных ситуаций), он будет возвращен в конец
    /// очереди и увеличен счетчик повторных попыток.
    /// В случае, если вернут Processing, то документ уйдет в очередь для обновления статуса
    /// В случае, если вернут Success, то будет отправлено событие о завершении
    /// </summary>
    internal class DocumentPublishProcessingBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IPublishService _publishService;
        private readonly DocumentPublishQueueSettings _settings;

        public DocumentPublishProcessingBackgroundService(
            IOptions<DocumentPublishQueueSettings> settings,
            ILogger<DocumentPublishProcessingBackgroundService> logger,
            IPublishService publishService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _publishService = publishService ?? throw new ArgumentNullException(nameof(publishService));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < _settings.ConsumersCount; i++)
                {
                    var consumerThread = new Thread(() => MakeConsumer(cancellationToken)) {IsBackground = true};
                    consumerThread.Start();
                }
            }, cancellationToken);
        }

        private void MakeConsumer(CancellationToken cancellationToken)
        {
            _publishService.DocumentPublishProcessingService.ProcessQueue(async (message, deliveryTag) =>
            {
                var msg = DeserializeMessage(message);
                await ProcessMessage(msg, cancellationToken);

                /*
                 * if Error occured.. and need to republish (Внештатные ситуации - Timeout сети и т.д) 
                 * Не относится к ситуации, когда возникает ошибка при формировании документа, в этом случае сразу ошибку возвращаем
                 *     _documentPublishProcessingService.MarkAsProcessed(deliveryTag);
                 *     if (_settings.MaxRetryCount > msg.RetryCount) {
                 *          msg.RetryCount ++
                 *          _documentPublishService.PublishMessage(msg);
                 *          return;
                 *     }
                 *     
                 * if Processing returned then need to publish to update queue
                 * 
                 *  var updateMessage = new PublishUpdateQueueMessage
                 *  {
                 *      UserId = msg.UserId,
                 *      RefId = Guid.NewGuid().ToString(),
                 *      TimeStamp = DateTime.Now
                 *  };
                 *  _documentPublishUpdateService.PublishMessage(updateMessage);
                 * 
                 * if Success, need to update document status, and push notification
                 */

                _publishService.DocumentPublishProcessingService.MarkAsProcessed(deliveryTag);
            }, RaiseException);
        }

        private static PublishQueueMessage DeserializeMessage(string message)
        {
            return JsonConvert.DeserializeObject<PublishQueueMessage>(message);
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stoppingToken"></param>
        /// <returns>must return http response message</returns>
        private static async Task ProcessMessage(PublishQueueMessage message, CancellationToken stoppingToken)
        {
            // make publish, get response... and return result
            await Task.Delay(0, stoppingToken);
        }

        private void RaiseException(Exception ex, ulong deliveryTag)
        {
            _logger.LogError(ex, "RaiseException" + deliveryTag);
        }
    }
}
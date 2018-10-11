using System;
using System.Threading;
using System.Threading.Tasks;
using Integration.Abstractions;
using Integration.Abstractions.QueueServices;
using Integration.EventMessages;
using Integration.Models;
using Microsoft.Extensions.Logging;

namespace Integration.EventHandlers
{
    public class DocumentPublishResultMessageEventHandler : BaseDocumentMessageEventHandler<
        DocumentPublishResultEventMessage,
        IDocumentPublishResultQueueService>
    {
        private readonly ILogger<DocumentPublishResultMessageEventHandler> _logger;

        public DocumentPublishResultMessageEventHandler(
            IPublishDocumentTaskRepository publishDocumentTaskRepository,
            ILogger<DocumentPublishResultMessageEventHandler> logger,
            INotifyService notifyService,
            IDocumentPublishResultQueueService documentPublishResultQueueService)
            : base(publishDocumentTaskRepository, logger, notifyService, documentPublishResultQueueService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<bool> OnMessageReceivedAsync(DocumentPublishResultEventMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var publishDocumentTask = await PublishDocumentTaskRepository.GetAsync(message.Id);

                if (publishDocumentTask == null)
                {
                    return false;
                }

                if (publishDocumentTask.IsDelivered)
                {
                    return true;
                }

                SavePublishTaskInfo(publishDocumentTask);
                await NotifyService.SendNotificationAsync(publishDocumentTask);

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Возникла ошибка при обработке сообщения");
                return false;
            }
        }

        /// <summary>
        /// Default Error handler..
        /// </summary>
        /// <param name="ex">Пойманная ошибка</param>
        /// <param name="publishMessage">Сообщение</param>
        protected override void RaiseException(Exception ex, DocumentPublishResultEventMessage publishMessage)
        {
            _logger.LogError(ex, "RaiseException" + publishMessage.Id);
        }

        private void SavePublishTaskInfo(PublishDocumentTask publishDocumentTask)
        {
            publishDocumentTask.IsDelivered = true;
            publishDocumentTask.UpdatedAt = DateTime.Now;
            _logger.LogDebug($"Delivered task id={publishDocumentTask.DocumentId}");
            PublishDocumentTaskRepository.Save(publishDocumentTask);
        }
    }
}
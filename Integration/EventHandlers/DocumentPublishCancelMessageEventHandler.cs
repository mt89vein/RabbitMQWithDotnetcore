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
    public class DocumentPublishCancelMessageEventHandler : BaseDocumentMessageEventHandler<
        DocumentPublishCancelEventMessage,
        IDocumentPublishCancelQueueService>
    {
        private readonly ILogger<DocumentPublishCancelMessageEventHandler> _logger;

        public DocumentPublishCancelMessageEventHandler(
            IPublishDocumentTaskRepository publishDocumentTaskRepository,
            ILogger<DocumentPublishCancelMessageEventHandler> logger,
            INotifyService notifyService,
            IDocumentPublishCancelQueueService documentPublishCancelQueueService)
            : base(publishDocumentTaskRepository, logger, notifyService, documentPublishCancelQueueService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<bool> OnMessageReceivedAsync(DocumentPublishCancelEventMessage message,
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

                if (publishDocumentTask.IsFinished)
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
        protected override void RaiseException(Exception ex, DocumentPublishCancelEventMessage publishMessage)
        {
            _logger.LogError(ex, "RaiseException" + publishMessage.Id);
        }

        private void SavePublishTaskInfo(PublishDocumentTask publishDocumentTask)
        {
            publishDocumentTask.State = PublishState.Canceled;
            publishDocumentTask.UpdatedAt = DateTime.Now;
            _logger.LogDebug($"id={publishDocumentTask.DocumentId} PublishDocumentTask.State= {publishDocumentTask.State}");
            PublishDocumentTaskRepository.Save(publishDocumentTask);
        }
    }
}
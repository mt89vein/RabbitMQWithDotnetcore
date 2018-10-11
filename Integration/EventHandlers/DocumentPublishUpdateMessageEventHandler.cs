using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Integration.Abstractions;
using Integration.Abstractions.QueueServices;
using Integration.EventMessages;
using Integration.Models;
using Microsoft.Extensions.Logging;

namespace Integration.EventHandlers
{
    public class DocumentPublishUpdateMessageEventHandler : BaseDocumentMessageEventHandler<
        DocumentPublishUpdateEventMessage,
        IDocumentPublishUpdateQueueService>
    {
        private readonly ILogger<DocumentPublishUpdateMessageEventHandler> _logger;

        public DocumentPublishUpdateMessageEventHandler(
            IPublishDocumentTaskRepository publishDocumentTaskRepository,
            ILogger<DocumentPublishUpdateMessageEventHandler> logger,
            INotifyService notifyService,
            IDocumentPublishUpdateQueueService documentPublishUpdateQueueService)
            : base(publishDocumentTaskRepository, logger, notifyService, documentPublishUpdateQueueService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<bool> OnMessageReceivedAsync(DocumentPublishUpdateEventMessage message,
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

                var documentPublicationInfo = await ProcessMessage(publishDocumentTask, message, cancellationToken);

                SavePublishTaskInfo(publishDocumentTask, documentPublicationInfo);

                await NotifyService.SendNotificationAsync(publishDocumentTask);

                if (documentPublicationInfo.ResultType == PublicationResultType.Processing)
                {
                    return false;
                }

                var documentPublishResultEventMessage = new DocumentPublishResultEventMessage
                {
                    UserId = message.UserId,
                    Id = message.Id,
                    CreatedAt = message.CreatedAt,
                    LoadId = documentPublicationInfo.LoadId,
                    ResultType = documentPublicationInfo.ResultType
                };

                EventBus.PublishMessage(documentPublishResultEventMessage);

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
        protected override void RaiseException(Exception ex, DocumentPublishUpdateEventMessage publishMessage)
        {
            _logger.LogError(ex, "RaiseException" + publishMessage.Id);
        }

        /// <summary>
        /// Сохранить изменения по задаче на публикацию
        /// </summary>
        /// <param name="publishDocumentTask"></param>
        /// <param name="documentPublicationInfo"></param>
        private void SavePublishTaskInfo(PublishDocumentTask publishDocumentTask,
            DocumentPublicationInfo documentPublicationInfo)
        {
            var publishDocumentTaskAttempt = new PublishDocumentTaskAttempt
            {
                PublishDocumentTaskId = publishDocumentTask.Id,
                Response = documentPublicationInfo.Response,
                Request = documentPublicationInfo.Request,
                HasEisExceptions = documentPublicationInfo.HasInnerExceptions,
                Result = documentPublicationInfo.ResultType
            };

            publishDocumentTask.State = ConvertToPublishState(documentPublicationInfo.ResultType);
            publishDocumentTask.HasEisExceptions = publishDocumentTask.HasEisExceptions ||
                                                 publishDocumentTaskAttempt.HasEisExceptions;
            publishDocumentTask.LoadId = documentPublicationInfo.LoadId;
            publishDocumentTask.RefId = documentPublicationInfo.RefId;
            publishDocumentTask.UpdatedAt = DateTime.Now;
            publishDocumentTask.PublishDocumentTaskAttempts.Add(publishDocumentTaskAttempt);
            PublishDocumentTaskRepository.Save(publishDocumentTask);
            _logger.LogDebug(
                $"id={publishDocumentTask.DocumentId} PublishDocumentTask.State= {publishDocumentTask.State} attempt result= {publishDocumentTaskAttempt.Result}");

            PublishState ConvertToPublishState(PublicationResultType resultType)
            {
                switch (resultType)
                {
                    case PublicationResultType.Success:
                        return PublishState.Published;
                    case PublicationResultType.Failed:
                        return PublishState.Failed;
                    case PublicationResultType.Processing:
                        return PublishState.Processing;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resultType));
                }
            }
        }

        /// <summary>
        /// Обработчик сообщения
        /// </summary>
        /// <param name="publishDocumentTask">Задача</param>
        /// <param name="message">Сообщение</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>must return http response message</returns>
        private async Task<DocumentPublicationInfo> ProcessMessage(PublishDocumentTask publishDocumentTask,
            DocumentPublishUpdateEventMessage message,
            CancellationToken cancellationToken)
        {
            /***
             * 
             * Perform update document state logic from another system
             *  
             * */
            await Task.Delay(0, cancellationToken);

            int? loadId = null;
            var result = (PublicationResultType)new Random().Next(0, 3);
            if (result == PublicationResultType.Success)
            {
                loadId = new Random().Next(1, int.MaxValue);
            }

            return new DocumentPublicationInfo(message.RefId, result, loadId, "document update request",
                "document update response");
        }
    }
}
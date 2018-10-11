using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Integration.Abstractions;
using Integration.Abstractions.QueueServices;
using Integration.EventMessages;
using Integration.IntegrationDocuments;
using Integration.Models;
using Microsoft.Extensions.Logging;

namespace Integration.EventHandlers
{
    public class DocumentPublishMessageEventHandler : BaseDocumentMessageEventHandler<DocumentPublishEventMessage,
        IDocumentPublishQueueService>
    {
        private readonly ILogger<DocumentPublishMessageEventHandler> _logger;

        public DocumentPublishMessageEventHandler(
            IPublishDocumentTaskRepository publishDocumentTaskRepository,
            ILogger<DocumentPublishMessageEventHandler> logger,
            INotifyService notifyService,
            IDocumentPublishQueueService documentPublishQueueService)
            : base(publishDocumentTaskRepository, logger, notifyService, documentPublishQueueService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<bool> OnMessageReceivedAsync(DocumentPublishEventMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var publishDocumentTask = await PublishDocumentTaskRepository.GetAsync(message.Id);

                if (publishDocumentTask == null)
                {
                    publishDocumentTask = new PublishDocumentTask(message);
                    PublishDocumentTaskRepository.Save(publishDocumentTask);
                    await NotifyService.SendNotificationAsync(publishDocumentTask);
                }

                if (!CheckIsNeedToProcessMessage(publishDocumentTask))
                {
                    return true;
                }

                var documentPublicationInfo = await ProcessMessage(message, cancellationToken);

                SavePublishTaskInfo(publishDocumentTask, documentPublicationInfo);

                await NotifyService.SendNotificationAsync(publishDocumentTask);

                if (documentPublicationInfo.ResultType == PublicationResultType.Processing)
                {
                    var updateMessage = CreatePublishUpdateMessage(message, documentPublicationInfo.RefId);
                    EventBus.PublishMessage(updateMessage);

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
        protected override void RaiseException(Exception ex, DocumentPublishEventMessage publishMessage)
        {
            _logger.LogError(ex, "RaiseException" + publishMessage.Id);
        }

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
                    case PublicationResultType.XmlValidationError:
                        return PublishState.XmlValidationError;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resultType));
                }
            }
        }

        /// <summary>
        /// Проверить, нужно ли обрабатывать сообщение
        /// </summary>
        /// <param name="publishDocumentTask">Задача</param>
        /// <returns>True, если сообщение еще не было обработано</returns>
        private static bool CheckIsNeedToProcessMessage(PublishDocumentTask publishDocumentTask)
        {
            if (publishDocumentTask.IsFinished)
            {
                return false;
            }

            // Processing.. no actions needed, just return false.
            if (publishDocumentTask.State == PublishState.Processing)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверить документ на наличие ошибок перед отправкой
        /// </summary>
        /// <param name="xmlDocument">Сериализованный документ</param>
        /// <returns></returns>
        private bool ValidateXmlDocument(object xmlDocument)
        {
            // validate logic
            return true;
        }

        /// <summary>
        /// Конвертировать POCO в XML
        /// </summary>
        /// <param name="xmlDocument">POCO xml класс</param>
        /// <returns>Сериализованный документ</returns>
        private static object SerializeXmlDocument(IOuterXmlDocument xmlDocument)
        {
            // create xml serializer
            // use memory stream...
            // return serialized document
            return xmlDocument.ToString();
        }

        /// <summary>
        /// Отправить документ во внешний сервис
        /// </summary>
        /// <param name="serializedDocument">Сериализованный документ</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат публикации</returns>
        private Task<DocumentPublicationInfo> SendXmlDocumentAsync(object serializedDocument,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int? loadId = null;
            var publicationResult = (PublicationResultType) new Random().Next(0, 3);
            if (publicationResult == PublicationResultType.Success)
            {
                loadId = new Random().Next(1, int.MaxValue);
            }

            var documentPublicationInfo = new DocumentPublicationInfo(Guid.NewGuid().ToString(), publicationResult,
                loadId, serializedDocument.ToString(), "document publish response");

            return Task.FromResult(documentPublicationInfo);
        }

        /// <summary>
        /// Обработать сообщение
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>must return http response eventMessage</returns>
        private async Task<DocumentPublicationInfo> ProcessMessage(DocumentPublishEventMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(0, cancellationToken);
            var loadedAttachments = await LoadAttachmentsAsync(message, cancellationToken);
            var outerSystemFormat = await MapToOuterSystemFormatAsync(message, loadedAttachments, cancellationToken);
            var serializedXmlDocument = SerializeXmlDocument(outerSystemFormat);
            var isValidXmlDocument = ValidateXmlDocument(serializedXmlDocument);

            if (!isValidXmlDocument)
            {
                return new DocumentPublicationInfo(null, PublicationResultType.XmlValidationError, null,
                    serializedXmlDocument.ToString(), null);
            }

            return await SendXmlDocumentAsync(serializedXmlDocument, cancellationToken);
        }


        /// <summary>
        /// Загрузить во внешнюю систему файлы вложений
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns></returns>
        private Task<IEnumerable<string>> LoadAttachmentsAsync(DocumentPublishEventMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // load attachments before build xml document
            return Task.FromResult(Enumerable.Empty<string>());
        }

        /// <summary>
        /// Сформировать POCO класс формата внешней системы
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="attachments">Результат предварительной загрузки вложений</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>POCO xml класс</returns>
        private Task<IOuterXmlDocument> MapToOuterSystemFormatAsync(DocumentPublishEventMessage message,
            IEnumerable<string> attachments, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IOuterXmlDocument xmlDocument;
            switch (message.DocumentType)
            {
                case DocumentType.One:
                    xmlDocument = new ConcreteXmlDocumentTypeOne();
                    break;
                case DocumentType.Two:
                    xmlDocument = new ConcreteXmlDocumentTypeTwo();
                    break;
                case DocumentType.Three:
                    xmlDocument = new ConcreteXmlDocumentTypeThree();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(DocumentType));
            }
            // use mapper to map from domain document to xml POCO document
            return Task.FromResult(xmlDocument);
        }

        /// <summary>
        /// Поставить на очередь на обновление результата публикации
        /// </summary>
        /// <param name="message">Сообщение с данными для апдейта</param>
        /// <param name="refId">Идентификатор по которому можно запросить результат публикации</param>
        /// <returns>Создает задачу на обновление результата публикации</returns>
        private DocumentPublishUpdateEventMessage CreatePublishUpdateMessage(
            DocumentPublishEventMessage message, string refId)
        {
            return new DocumentPublishUpdateEventMessage
            {
                Id = message.Id,
                UserId = message.UserId,
                RefId = refId,
                CreatedAt = DateTime.Now
            };
        }
    }
}
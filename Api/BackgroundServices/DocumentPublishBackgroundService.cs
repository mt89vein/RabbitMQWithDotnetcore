using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Services;
using Domain;
using Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.QueueServices;
using MQ.Abstractions.Repositories;
using MQ.Configuration;
using MQ.Messages;
using MQ.Models;

namespace Api.BackgroundServices
{
    /// <summary>
    /// Сервис фоновых задач для публикации документа во внешнюю систему
    /// </summary>
    public class DocumentPublishBackgroundService : BackgroundService
    {
        private readonly ILogger<DocumentPublishBackgroundService> _logger;
        private readonly INotifyService _notifyService;
        private readonly List<IServiceScope> _scopes;
        private readonly IServiceProvider _serviceProvider;
        private readonly DocumentPublishQueueSettings _settings;

        public DocumentPublishBackgroundService(
            INotifyService notifyService,
            IOptions<DocumentPublishQueueSettings> settings,
            ILogger<DocumentPublishBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _scopes = new List<IServiceScope>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < _settings.ConsumersCount; i++)
                {
                    var scope = _serviceProvider.CreateScope();
                    var publishDocumentTaskRepository = scope.ServiceProvider
                        .GetRequiredService<IPublishDocumentTaskRepository>();
                    var documentPublishQueueService = scope.ServiceProvider
                        .GetRequiredService<IDocumentPublishQueueService>();
                    var consumerThread = new Thread(() => MakeConsumer(publishDocumentTaskRepository,
                        documentPublishQueueService, cancellationToken))
                    {
                        IsBackground = true
                    };
                    _scopes.Add(scope);
                    consumerThread.Start();
                }
            }, cancellationToken);
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

        /// <summary>
        /// Default Error handler..
        /// </summary>
        /// <param name="ex">Пойманная ошибка</param>
        /// <param name="publishMessage">Сообщение</param>
        protected virtual void RaiseException(Exception ex, DocumentPublishEventMessage publishMessage)
        {
            _logger.LogError(ex, "RaiseException" + publishMessage.Id);
        }

        public override void Dispose()
        {
            foreach (var scope in _scopes)
            {
                scope?.Dispose();
            }
            base.Dispose();
        }

        private void MakeConsumer(IPublishDocumentTaskRepository publishDocumentTaskRepository,
            IDocumentPublishQueueService documentPublishQueueService,
            CancellationToken cancellationToken)
        {
            documentPublishQueueService.ProcessQueue<DocumentPublishEventMessage>(async message =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var publishDocumentTask = await publishDocumentTaskRepository.GetAsync(message.Id);

                    if (publishDocumentTask == null)
                    {
                        publishDocumentTask = new PublishDocumentTask(message);
                        publishDocumentTaskRepository.Save(publishDocumentTask);
                        await Task.Delay(300, cancellationToken);
                        await _notifyService.SendNotificationAsync(publishDocumentTask);
                    }

                    if (!CheckIsNeedToProcessMessage(publishDocumentTask))
                    {
                        return true;
                    }

                    var documentPublicationInfo = await ProcessMessage(message, cancellationToken);

                    SavePublishTaskInfo(publishDocumentTaskRepository, publishDocumentTask, documentPublicationInfo);

                    await _notifyService.SendNotificationAsync(publishDocumentTask);

                    if (documentPublicationInfo.ResultType == PublicationResultType.Processing)
                    {
                        var updateMessage = CreatePublishUpdateMessage(message, documentPublicationInfo.RefId);
                        documentPublishQueueService.PublishMessage(updateMessage);

                        return false;
                    }

                    var documentPublishResultEventMessage = new DocumentPublishResultEventMessage
                    {
                        UserId = message.UserId,
                        Id = message.Id,
                        CreatedAt = message.CreatedAt,
                        LoadId = documentPublicationInfo.LoadId,
                        ResultType = documentPublicationInfo.ResultType,
                    };

                    documentPublishQueueService.PublishMessage(documentPublishResultEventMessage);

                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Возникла ошибка при обработке сообщения");
                    return false;
                }
            }, RaiseException);
        }

        private void SavePublishTaskInfo(IPublishDocumentTaskRepository publishDocumentTaskRepository,
            PublishDocumentTask publishDocumentTask, DocumentPublicationInfo documentPublicationInfo)
        {
            var publishDocumentTaskAttempt = new PublishDocumentTaskAttempt
            {
                PublishDocumentTaskId = publishDocumentTask.Id,
                Response = documentPublicationInfo.Response,
                Request = documentPublicationInfo.Request,
                IsHasEisError = documentPublicationInfo.IsHasEisError,
                Result = documentPublicationInfo.ResultType
            };

            publishDocumentTask.State = ConvertToPublishState(documentPublicationInfo.ResultType);
            publishDocumentTask.IsHasEisErrors = publishDocumentTask.IsHasEisErrors ||
                                                 publishDocumentTaskAttempt.IsHasEisError;
            publishDocumentTask.LoadId = documentPublicationInfo.LoadId;
            publishDocumentTask.RefId = documentPublicationInfo.RefId;
            publishDocumentTask.UpdatedAt = DateTime.Now;
            publishDocumentTask.PublishDocumentTaskAttempts.Add(publishDocumentTaskAttempt);
            publishDocumentTaskRepository.Save(publishDocumentTask);
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

            await Task.Delay(5000, cancellationToken);
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
    }
}
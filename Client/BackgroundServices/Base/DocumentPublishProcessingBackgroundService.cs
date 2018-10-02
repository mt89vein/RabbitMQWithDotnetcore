using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Base;
using MQ.Abstractions.Messages;
using MQ.Abstractions.Repositories;
using MQ.Configuration.Base;
using MQ.Models;

namespace Client.BackgroundServices.Base
{
    /// <summary>
    /// Воркер фоновых задач для первичной публикации документа
    /// В случае, если документ не будет опубликован (в случае возникновения внештатных ситуаций), он будет возвращен в конец
    /// очереди и увеличен счетчик повторных попыток.
    /// В случае, если вернут Processing, то документ уйдет в очередь для обновления статуса
    /// В случае, если вернут Success или Failed, то будет отправлено событие о завершении
    /// </summary>
    public abstract class DocumentPublishProcessingBackgroundService<
        TDocumentPublishConsumerService,
        TDocumentPublishUpdateProducerService,
        TDocumentPublishConsumerServiceSettings,
        TPublishMessage,
        TPublishUpdateMessage,
        TXmlDocumentType> : BackgroundService
        where TDocumentPublishConsumerService : class, IConsumerService
        where TDocumentPublishUpdateProducerService : class, IProducerService
        where TDocumentPublishConsumerServiceSettings : DocumentPublishQueueSettings, new()
        where TPublishMessage : DocumentPublishEventMessage
        where TPublishUpdateMessage : DocumentPublishUpdateEventMessage, new()
        where TXmlDocumentType : IOuterXmlDocument, new()
    {
        private readonly ILogger<DocumentPublishProcessingBackgroundService<
            TDocumentPublishConsumerService,
            TDocumentPublishUpdateProducerService,
            TDocumentPublishConsumerServiceSettings,
            TPublishMessage,
            TPublishUpdateMessage,
            TXmlDocumentType>> _logger;

        private readonly List<IServiceScope> _scopes;

        private readonly IServiceProvider _serviceProvider;
        private readonly TDocumentPublishConsumerServiceSettings _settings;

        protected DocumentPublishProcessingBackgroundService(
            IOptions<TDocumentPublishConsumerServiceSettings> settings,
            ILogger<DocumentPublishProcessingBackgroundService<TDocumentPublishConsumerService,
                TDocumentPublishUpdateProducerService, TDocumentPublishConsumerServiceSettings, TPublishMessage,
                TPublishUpdateMessage, TXmlDocumentType>> logger,
            IServiceProvider serviceProvider)
        {
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
                    var publishConsumerService = scope.ServiceProvider
                        .GetRequiredService<TDocumentPublishConsumerService>();
                    var publishUpdateProducerService = scope.ServiceProvider
                        .GetRequiredService<TDocumentPublishUpdateProducerService>();
                    var consumerThread = new Thread(() => MakeConsumer(publishDocumentTaskRepository,
                        publishConsumerService,
                        publishUpdateProducerService, cancellationToken))
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
        protected abstract Task<IEnumerable<string>> LoadAttachmentsAsync(TPublishMessage message,
            CancellationToken cancellationToken);

        /// <summary>
        /// Сформировать POCO класс формата внешней системы
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="attachments">Результат предварительной загрузки вложений</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>POCO xml класс</returns>
        protected abstract Task<TXmlDocumentType> MapToOuterSystemFormatAsync(TPublishMessage message,
            IEnumerable<string> attachments, CancellationToken cancellationToken);

        /// <summary>
        /// Поставить на очередь на обновление результата публикации
        /// </summary>
        /// <param name="message">Сообщение с данными для апдейта</param>
        /// <param name="refId">Идентификатор по которому можно запросить результат публикации</param>
        /// <returns>Создает задачу на обновление результата публикации</returns>
        protected virtual TPublishUpdateMessage CreatePublishUpdateMessage(TPublishMessage message, string refId)
        {
            return new TPublishUpdateMessage
            {
                Id = message.Id,
                UserId = message.UserId,
                DocumentType = message.DocumentType,
                DocumentId = message.DocumentId,
                DocumentRevision = message.DocumentRevision,
                UserInputData = message.UserInputData,
                RefId = refId,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Default Error handler..
        /// </summary>
        /// <param name="ex">Пойманная ошибка</param>
        /// <param name="publishMessage">Сообщение</param>
        protected virtual void RaiseException(Exception ex, TPublishMessage publishMessage)
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
            TDocumentPublishConsumerService documentPublishConsumerService,
            TDocumentPublishUpdateProducerService documentPublishUpdateProducerService,
            CancellationToken cancellationToken)
        {
            documentPublishConsumerService.ProcessQueue<TPublishMessage>(async message =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var publishDocumentTask = await publishDocumentTaskRepository.Get(message.Id);

                    if (publishDocumentTask == null)
                    {
                        return false;
                    }

                    if (!CheckIsNeedToProcessMessage(publishDocumentTask))
                    {
                        return true;
                    }

                    var documentPublicationInfo = await ProcessMessage(message, cancellationToken);

                    if (documentPublicationInfo.ResultType == PublicationResultType.Processing)
                    {
                        var updateMessage = CreatePublishUpdateMessage(message, documentPublicationInfo.RefId);
                        documentPublishUpdateProducerService.PublishMessage(updateMessage);
                    }

                    publishDocumentTaskRepository.SavePublishAttempt(publishDocumentTask, documentPublicationInfo);
                    //SendNotification(message, documentPublicationInfo);

                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Возникла ошибка при обработке сообщения");
                    return false;
                }
            }, RaiseException);
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
        private object SerializeXmlDocument(TXmlDocumentType xmlDocument)
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
        private async Task<DocumentPublicationInfo> ProcessMessage(TPublishMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // working 
            //await Task.Delay(0, cancellationToken);

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
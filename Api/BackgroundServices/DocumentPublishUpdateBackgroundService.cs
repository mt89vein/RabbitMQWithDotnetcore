using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Services;
using Domain;
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
    /// Сервис фоновых задач для опроса результата публикации документа у внешней системы
    /// </summary>
    public class DocumentPublishUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger<DocumentPublishUpdateBackgroundService> _logger;
        private readonly INotifyService _notifyService;
        private readonly IServiceProvider _provider;
        private readonly List<IServiceScope> _scopes;
        private readonly DocumentPublishUpdateQueueSettings _settings;

        public DocumentPublishUpdateBackgroundService(
            INotifyService notifyService,
            IServiceProvider provider,
            IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService> logger)
        {
            _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _provider = provider;
            _scopes = new List<IServiceScope>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < _settings.ConsumersCount; i++)
                {
                    var scope = _provider.CreateScope();
                    var publishDocumentTaskRepository = scope.ServiceProvider
                        .GetRequiredService<IPublishDocumentTaskRepository>();
                    var documentPublishUpdateProcessingService = scope.ServiceProvider
                        .GetRequiredService<IDocumentPublishUpdateQueueService>();
                    var consumerThread =
                        new Thread(() => MakeConsumer(publishDocumentTaskRepository,
                            documentPublishUpdateProcessingService, cancellationToken))
                        {
                            IsBackground = true
                        };
                    _scopes.Add(scope);
                    consumerThread.Start();
                }
            }, cancellationToken);
        }

        private void MakeConsumer(IPublishDocumentTaskRepository publishDocumentTaskRepository,
            IDocumentPublishUpdateQueueService documentPublishUpdateQueueService,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            documentPublishUpdateQueueService.ProcessQueue<DocumentPublishUpdateEventMessage>(async message =>
            {
                try
                {
                    var publishDocumentTask = await publishDocumentTaskRepository.GetAsync(message.Id);
                    if (publishDocumentTask == null)
                    {
                        return false;
                    }

                    if (publishDocumentTask.IsFinished)
                    {
                        return true;
                    }

                    var documentPublicationInfo = await ProcessMessage(publishDocumentTask, message, cancellationToken);

                    SavePublishTaskInfo(publishDocumentTaskRepository, publishDocumentTask, documentPublicationInfo);

                    await _notifyService.SendNotificationAsync(publishDocumentTask);

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
                        ResultType = documentPublicationInfo.ResultType,
                    };

                    documentPublishUpdateQueueService.PublishMessage(documentPublishResultEventMessage);

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
        /// Сохранить изменения по задаче на публикацию
        /// </summary>
        /// <param name="publishDocumentTaskRepository"></param>
        /// <param name="publishDocumentTask"></param>
        /// <param name="documentPublicationInfo"></param>
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
        protected virtual async Task<DocumentPublicationInfo> ProcessMessage(PublishDocumentTask publishDocumentTask,
            DocumentPublishUpdateEventMessage message,
            CancellationToken cancellationToken)
        {
            /***
             * 
             * Perform update document state logic from another system
             *  
             * */
            await Task.Delay(1500, cancellationToken);

            int? loadId = null;
            var result = (PublicationResultType) new Random().Next(0, 3);
            if (result == PublicationResultType.Success)
            {
                loadId = new Random().Next(1, int.MaxValue);
            }

            return new DocumentPublicationInfo(message.RefId, result, loadId, "document update request",
                "document update response");
        }

        /// <summary>
        /// Обработчик внутренних ошибок в работе с очередями
        /// </summary>
        /// <param name="ex">Ошибка</param>
        /// <param name="message">Сообщение, которое не удалось обработать</param>
        protected void RaiseException(Exception ex, DocumentPublishUpdateEventMessage message)
        {
            _logger.LogError(ex, "RaiseException" + message.Id);
        }

        public override void Dispose()
        {
            foreach (var scope in _scopes)
            {
                scope?.Dispose();
            }
            base.Dispose();
        }
    }
}
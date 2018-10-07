using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Services;
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
    /// Сервис фоновых задач для отмены публикации документа во внешнюю систему
    /// </summary>
    public class DocumentPublishCancelBackgroundService : BackgroundService
    {
        private readonly ILogger<DocumentPublishCancelBackgroundService> _logger;
        private readonly IServiceProvider _provider;
        private readonly DocumentPublishCancelQueueSettings _settings;
        private readonly List<IServiceScope> _scopes;
        private readonly INotifyService _notifyService;

        public DocumentPublishCancelBackgroundService(
            INotifyService notifyService,
            IServiceProvider provider,
            IOptions<DocumentPublishCancelQueueSettings> settings,
            ILogger<DocumentPublishCancelBackgroundService> logger)
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
                    var documentPublishCancelQueueService = scope.ServiceProvider
                        .GetRequiredService<IDocumentPublishCancelQueueService>();
                    var consumerThread =
                        new Thread(() => MakeConsumer(publishDocumentTaskRepository,
                            documentPublishCancelQueueService, cancellationToken))
                        {
                            IsBackground = true
                        };
                    _scopes.Add(scope);
                    consumerThread.Start();
                }
            }, cancellationToken);
        }

        private void MakeConsumer(IPublishDocumentTaskRepository publishDocumentTaskRepository,
            IDocumentPublishCancelQueueService documentPublishCancelQueueService,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            documentPublishCancelQueueService.ProcessQueue<DocumentPublishCancelEventMessage>(async message =>
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

                    SavePublishTaskInfo(publishDocumentTaskRepository, publishDocumentTask);
                    await _notifyService.SendNotificationAsync(publishDocumentTask);

                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Возникла ошибка при обработке сообщения");
                    return false;
                }
            }, RaiseException);
        }

        private void SavePublishTaskInfo(IPublishDocumentTaskRepository publishDocumentTaskRepository, PublishDocumentTask publishDocumentTask)
        {
            publishDocumentTask.State = PublishState.Canceled;
            publishDocumentTask.UpdatedAt = DateTime.Now;
            _logger.LogDebug($"id={publishDocumentTask.DocumentId} PublishDocumentTask.State= {publishDocumentTask.State}");
            publishDocumentTaskRepository.Save(publishDocumentTask);
        }

        /// <summary>
        /// Обработчик внутренних ошибок в работе с очередями
        /// </summary>
        /// <param name="ex">Ошибка</param>
        /// <param name="message">Сообщение, которое не удалось обработать</param>
        protected void RaiseException(Exception ex, DocumentPublishCancelEventMessage message)
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
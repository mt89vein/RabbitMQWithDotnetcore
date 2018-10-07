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
    /// Сервис фоновых задач для обработки результата публикации документа во внешнюю систему
    /// </summary>
    public class DocumentPublishResultBackgroundService : BackgroundService
    {
        private readonly ILogger<DocumentPublishResultBackgroundService> _logger;
        private readonly IServiceProvider _provider;
        private readonly DocumentPublishResultQueueSettings _settings;
        private readonly List<IServiceScope> _scopes;
        private readonly INotifyService _notifyService;

        public DocumentPublishResultBackgroundService(
            INotifyService notifyService,
            IServiceProvider provider,
            IOptions<DocumentPublishResultQueueSettings> settings,
            ILogger<DocumentPublishResultBackgroundService> logger)
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
                    var documentPublishResultQueueService = scope.ServiceProvider
                        .GetRequiredService<IDocumentPublishResultQueueService>();
                    var consumerThread =
                        new Thread(() => MakeConsumer(publishDocumentTaskRepository,
                            documentPublishResultQueueService, cancellationToken))
                        {
                            IsBackground = true
                        };
                    _scopes.Add(scope);
                    consumerThread.Start();
                }
            }, cancellationToken);
        }

        private void MakeConsumer(IPublishDocumentTaskRepository publishDocumentTaskRepository,
            IDocumentPublishResultQueueService documentPublishResultQueueService,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            documentPublishResultQueueService.ProcessQueue<DocumentPublishResultEventMessage>(async message =>
            {
                try
                {
                    var publishDocumentTask = await publishDocumentTaskRepository.GetAsync(message.Id);

                    if (publishDocumentTask == null)
                    {
                        return false;
                    }

                    if (publishDocumentTask.IsDelivered)
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
            publishDocumentTask.IsDelivered = true;
            publishDocumentTask.UpdatedAt = DateTime.Now;
            _logger.LogDebug($"Delivered task id={publishDocumentTask.DocumentId}");
            publishDocumentTaskRepository.Save(publishDocumentTask);
        }

        /// <summary>
        /// Обработчик внутренних ошибок в работе с очередями
        /// </summary>
        /// <param name="ex">Ошибка</param>
        /// <param name="message">Сообщение, которое не удалось обработать</param>
        protected void RaiseException(Exception ex, DocumentPublishResultEventMessage message)
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
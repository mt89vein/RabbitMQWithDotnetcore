using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Integration.Configuration;
using Integration.EventHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.BackgroundServices
{
    /// <summary>
    /// Сервис фоновых задач для публикации документа во внешнюю систему
    /// </summary>
    public class DocumentPublishBackgroundService : BackgroundService
    {
        private readonly IOptionsMonitor<DocumentPublishCancelQueueSettings> _documentPublishCancelQueueSettings;
        private readonly IOptionsMonitor<DocumentPublishQueueSettings> _documentPublishQueueSettings;
        private readonly IOptionsMonitor<DocumentPublishResultQueueSettings> _documentPublishResultQueueSettings;
        private readonly IOptionsMonitor<DocumentPublishUpdateQueueSettings> _documentPublishUpdateQueueSettings;
        private readonly ILogger<DocumentPublishBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Task, IServiceScope> _taskScopes;
        private CancellationTokenSource _cts;

        public DocumentPublishBackgroundService(
            ILogger<DocumentPublishBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptionsMonitor<DocumentPublishQueueSettings> documentPublishQueueSettings,
            IOptionsMonitor<DocumentPublishUpdateQueueSettings> documentPublishUpdateQueueSettings,
            IOptionsMonitor<DocumentPublishCancelQueueSettings> documentPublishCancelQueueSettings,
            IOptionsMonitor<DocumentPublishResultQueueSettings> documentPublishResultQueueSettings)
        {
            _documentPublishQueueSettings = documentPublishQueueSettings ??
                                            throw new ArgumentNullException(nameof(documentPublishQueueSettings));
            _documentPublishUpdateQueueSettings = documentPublishUpdateQueueSettings ??
                                                  throw new ArgumentNullException(
                                                      nameof(documentPublishUpdateQueueSettings));
            _documentPublishCancelQueueSettings = documentPublishCancelQueueSettings ??
                                                  throw new ArgumentNullException(
                                                      nameof(documentPublishCancelQueueSettings));
            _documentPublishResultQueueSettings = documentPublishResultQueueSettings ??
                                                  throw new ArgumentNullException(
                                                      nameof(documentPublishResultQueueSettings));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskScopes = new Dictionary<Task, IServiceScope>();
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _logger.LogDebug("DocumentPublishBackgroundService is starting.");

            cancellationToken.Register(() =>
                _logger.LogDebug("DocumentPublishBackgroundService is stopping."));

            _logger.LogDebug($"DocumentPublishBackgroundService doing background work.");

            Start<DocumentPublishMessageEventHandler>(_cts.Token, _documentPublishQueueSettings.CurrentValue.ConsumersCount);
            Start<DocumentPublishUpdateMessageEventHandler>(_cts.Token, _documentPublishUpdateQueueSettings.CurrentValue.ConsumersCount);
            Start<DocumentPublishCancelMessageEventHandler>(_cts.Token, _documentPublishCancelQueueSettings.CurrentValue.ConsumersCount);
            Start<DocumentPublishResultMessageEventHandler>(_cts.Token, _documentPublishResultQueueSettings.CurrentValue.ConsumersCount);

            _documentPublishQueueSettings.OnChange(async settings =>
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    _logger.LogDebug($"DocumentPublishBackgroundService is stopping.");
                    _cts.Cancel();
                    await CleanUp();
                    _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    _logger.LogDebug($"DocumentPublishBackgroundService is starting AGAIN");
                    Start<DocumentPublishMessageEventHandler>(_cts.Token, _documentPublishQueueSettings.CurrentValue.ConsumersCount);
                    Start<DocumentPublishUpdateMessageEventHandler>(_cts.Token, _documentPublishUpdateQueueSettings.CurrentValue.ConsumersCount);
                    Start<DocumentPublishCancelMessageEventHandler>(_cts.Token, _documentPublishCancelQueueSettings.CurrentValue.ConsumersCount);
                    Start<DocumentPublishResultMessageEventHandler>(_cts.Token, _documentPublishResultQueueSettings.CurrentValue.ConsumersCount);
                }
            });

            return Task.CompletedTask;
        }

        private async Task CleanUp()
        {
            _logger.LogDebug($"CLEAN UP STARTED");
            await Task.Delay(5000);
            foreach (var taskScope in _taskScopes)
            {
                taskScope.Key?.Dispose();
                taskScope.Value?.Dispose();
            }
            _taskScopes.Clear();
            _logger.LogDebug($"CLEAN UP ENDED");
        }


        private void Start<TDocumentPublishMessageEventHandler>(CancellationToken cancellationToken, int consumerCount)
            where TDocumentPublishMessageEventHandler : IBaseDocumentMessageEventHandler
        {
            for (var i = 0; i < consumerCount; i++)
            {
                var scope = _serviceProvider.CreateScope();
                var task = Task.Factory.StartNew(() => scope.ServiceProvider.GetRequiredService<TDocumentPublishMessageEventHandler>()
                    .Start(cancellationToken), TaskCreationOptions.LongRunning);
                _taskScopes.Add(task, scope);
            }
        }

        public override void Dispose()
        {
            foreach (var taskScope in _taskScopes)
            {
                taskScope.Value?.Dispose();
                taskScope.Key?.Dispose();
            }
            base.Dispose();
        }
    }
}
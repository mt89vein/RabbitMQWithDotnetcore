using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;
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
    public abstract class DocumentPublishUpdateBackgroundService<
        TDocumentPublishUpdateConsumerService,
        TDocumentPublishUpdateConsumerServiceSettings,
        TPublishUpdateMessage> : BackgroundService
        where TDocumentPublishUpdateConsumerService : class, IConsumerService
        where TDocumentPublishUpdateConsumerServiceSettings : DocumentPublishUpdateQueueSettings, new()
        where TPublishUpdateMessage : DocumentPublishUpdateEventMessage
    {
        private readonly ILogger<DocumentPublishUpdateBackgroundService<
            TDocumentPublishUpdateConsumerService,
            TDocumentPublishUpdateConsumerServiceSettings,
            TPublishUpdateMessage>> _logger;

        private readonly IServiceProvider _provider;
        private readonly DocumentPublishUpdateQueueSettings _settings;
        private readonly List<IServiceScope> _scopes;

        protected DocumentPublishUpdateBackgroundService(
            IServiceProvider provider,
            IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService<TDocumentPublishUpdateConsumerService,
                TDocumentPublishUpdateConsumerServiceSettings, TPublishUpdateMessage>> logger)
        {
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
                        .GetRequiredService<TDocumentPublishUpdateConsumerService>();
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
            TDocumentPublishUpdateConsumerService documentPublishUpdateConsumerService,
            CancellationToken cancellationToken)
        {
            documentPublishUpdateConsumerService.ProcessQueue<TPublishUpdateMessage>(async message =>
            {
                try
                {
                    var publishDocumentTask = await publishDocumentTaskRepository.Get(message.Id);

                    if (publishDocumentTask == null)
                    {
                        return false;
                    }

                    if (publishDocumentTask.IsFinished)
                    {
                        return true;
                    }

                    var documentPublicationInfo = await ProcessMessage(publishDocumentTask, message, cancellationToken);

                    var processingFinished = documentPublicationInfo.ResultType != PublicationResultType.Processing;

                    publishDocumentTaskRepository.SavePublishAttempt(publishDocumentTask, documentPublicationInfo);

                    //if (processingFinished)
                    //{
                    //    SendNotification(documentPublicationInfo);
                    //}

                    return processingFinished;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Возникла ошибка при обработке сообщения");
                    return false;
                }
            }, RaiseException);
        }

        /// <summary>
        /// Обработчик сообщения
        /// </summary>
        /// <param name="publishDocumentTask">Задача</param>
        /// <param name="message">Сообщение</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>must return http response message</returns>
        protected virtual async Task<DocumentPublicationInfo> ProcessMessage(PublishDocumentTask publishDocumentTask,
            TPublishUpdateMessage message,
            CancellationToken cancellationToken)
        {
            /***
             * 
             * Perform update document state logic from another system
             *  
             * */
            await Task.Delay(1500, cancellationToken);

            int? loadId = null;
            var result = (PublicationResultType)new Random().Next(0, 3);
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
        protected void RaiseException(Exception ex, TPublishUpdateMessage message)
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
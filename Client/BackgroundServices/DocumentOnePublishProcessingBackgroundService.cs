using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Client.BackgroundServices.Base;
using Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishServices;
using MQ.Abstractions.Messages;
using MQ.Abstractions.Producers.UpdateServices;
using MQ.Configuration.Consumers.PublishSettings;

namespace Client.BackgroundServices
{
    public sealed class DocumentOnePublishProcessingBackgroundService
        : DocumentPublishProcessingBackgroundService<
            IDocumentOnePublishConsumerService,
            IDocumentOnePublishUpdateProducerService,
            DocumentOnePublishConsumerServiceSettings,
            DocumentPublishEventMessage,
            DocumentPublishUpdateEventMessage,
            ConcreteXmlDocumentTypeOne>
    {

        private readonly ILogger<DocumentPublishProcessingBackgroundService<IDocumentOnePublishConsumerService,
            IDocumentOnePublishUpdateProducerService, DocumentOnePublishConsumerServiceSettings,
            DocumentPublishEventMessage, DocumentPublishUpdateEventMessage, ConcreteXmlDocumentTypeOne>> _logger;

        public DocumentOnePublishProcessingBackgroundService(
            IOptions<DocumentOnePublishConsumerServiceSettings> settings,
            ILogger<DocumentPublishProcessingBackgroundService<IDocumentOnePublishConsumerService,
                IDocumentOnePublishUpdateProducerService, DocumentOnePublishConsumerServiceSettings,
                DocumentPublishEventMessage, DocumentPublishUpdateEventMessage, ConcreteXmlDocumentTypeOne>> logger,
            IServiceProvider serviceProvider)
            : base(settings, logger, serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task<IEnumerable<string>> LoadAttachmentsAsync(DocumentPublishEventMessage message, CancellationToken cancellationToken)
        {
            // TODO: loading attachments before the document.
           // get document attachmentsLoadTasks.. and try to publish, if ok, return loadResults
           _logger.LogDebug($"Load attachmentsAsync for {message.Id}");
            return Task.FromResult(Enumerable.Empty<string>());
        }

        protected override Task<ConcreteXmlDocumentTypeOne> MapToOuterSystemFormatAsync(DocumentPublishEventMessage message, IEnumerable<string> loadedAttachments, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"MapToOuterSystemFormat for {message.Id}");
            return Task.FromResult(new ConcreteXmlDocumentTypeOne());
        }
    }
}
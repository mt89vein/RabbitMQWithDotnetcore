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
    public sealed class DocumentTwoPublishProcessingBackgroundService
        : DocumentPublishProcessingBackgroundService<
            IDocumentTwoPublishConsumerService,
            IDocumentTwoPublishUpdateProducerService,
            DocumentTwoPublishConsumerServiceSettings,
            DocumentPublishEventMessage,
            DocumentPublishUpdateEventMessage,
            ConcreteXmlDocumentTypeTwo>
    {
        public DocumentTwoPublishProcessingBackgroundService(
            IOptions<DocumentTwoPublishConsumerServiceSettings> settings,
            ILogger<DocumentPublishProcessingBackgroundService<IDocumentTwoPublishConsumerService,
                IDocumentTwoPublishUpdateProducerService, DocumentTwoPublishConsumerServiceSettings,
                DocumentPublishEventMessage, DocumentPublishUpdateEventMessage, ConcreteXmlDocumentTypeTwo>> logger,
            IServiceProvider serviceProvider)
            : base(settings, logger, serviceProvider)
        {
        }

        protected override Task<IEnumerable<string>> LoadAttachmentsAsync(DocumentPublishEventMessage message, CancellationToken cancellationToken)
        {
            // TODO: loading attachments before the document.
            // get document attachmentsLoadTasks.. and try to publish, if ok, return loadResults
            return Task.FromResult(Enumerable.Empty<string>());
        }

        protected override Task<ConcreteXmlDocumentTypeTwo> MapToOuterSystemFormatAsync(DocumentPublishEventMessage message, IEnumerable<string> attachments,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new ConcreteXmlDocumentTypeTwo());
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishUpdateServices;
using MQ.Abstractions.Messages;
using MQ.Configuration.Consumers.PublishUpdateSettings;

namespace Client
{
    public class DocumentTwoPublishUpdateProcessingBackgroundService :
        DocumentPublishUpdateBackgroundService<IDocumentTwoPublishUpdateConsumerService,
            DocumentTwoPublishUpdateConsumerServiceSettings, DocumentPublishUpdateEventMessage>
    {
        public DocumentTwoPublishUpdateProcessingBackgroundService(
            IServiceProvider provider,
            IOptions<DocumentTwoPublishUpdateConsumerServiceSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService<IDocumentTwoPublishUpdateConsumerService,
                DocumentTwoPublishUpdateConsumerServiceSettings, DocumentPublishUpdateEventMessage>> logger)
            : base(provider, settings, logger)
        {
        }

        protected override async Task<DocumentPublicationInfo> ProcessMessage(DocumentPublishUpdateEventMessage message,
            CancellationToken cancellationToken)
        {
            await Task.Delay(0, cancellationToken);

            int? loadId = null;
            var result = (PublicationResultType) new Random().Next(0, 3);
            if (result == PublicationResultType.Success)
            {
                loadId = new Random().Next(1, int.MaxValue);
            }

            return new DocumentPublicationInfo(message.RefId, result, loadId);
        }
    }
}
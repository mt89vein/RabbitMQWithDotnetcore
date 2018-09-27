using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishServices;
using MQ.Abstractions.Messages;
using MQ.Abstractions.Producers.UpdateServices;
using MQ.Configuration.Consumers.PublishSettings;

namespace Client
{
    public class DocumentOnePublishProcessingBackgroundService
        : DocumentPublishProcessingBackgroundService<
            IDocumentOnePublishConsumerService,
            IDocumentOnePublishUpdateProducerService,
            DocumentOnePublishConsumerServiceSettings,
            DocumentPublishEventMessage,
            DocumentPublishUpdateEventMessage>
    {
        public DocumentOnePublishProcessingBackgroundService(
            IOptions<DocumentOnePublishConsumerServiceSettings> settings,
            ILogger<DocumentPublishProcessingBackgroundService<IDocumentOnePublishConsumerService,
                IDocumentOnePublishUpdateProducerService, DocumentOnePublishConsumerServiceSettings,
                DocumentPublishEventMessage, DocumentPublishUpdateEventMessage>> logger,
            IServiceProvider serviceProvider)
            : base(settings, logger, serviceProvider)
        {
        }

        protected override async Task<DocumentPublicationInfo> ProcessMessage(DocumentPublishEventMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            /*
             * build xml
             * publish
             * get response... 
             * log to db
             * return DocumentPublicationInfo
             */
            await Task.Delay(0, cancellationToken);

            int? loadId = null;
            var result = (PublicationResultType) new Random().Next(0, 3);
            if (result == PublicationResultType.Success)
            {
                loadId = new Random().Next(1, int.MaxValue);
            }

            return new DocumentPublicationInfo(new Guid().ToString(), result, loadId);
        }

        protected override void SendNotification(DocumentPublicationInfo documentPublicationInfo)
        {
            Logger.LogInformation($"document notification, refId is: {documentPublicationInfo.RefId}");
        }

        protected override void SavePublishResult(DocumentPublicationInfo documentPublicationInfo)
        {
            Logger.LogInformation($"document save result, refId is: {documentPublicationInfo.RefId}");
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Client.BackgroundServices.Base;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishUpdateServices;
using MQ.Abstractions.Messages;
using MQ.Configuration.Consumers.PublishUpdateSettings;
using MQ.Models;

namespace Client.BackgroundServices
{
    public sealed class DocumentTwoPublishUpdateProcessingBackgroundService :
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
    }
}
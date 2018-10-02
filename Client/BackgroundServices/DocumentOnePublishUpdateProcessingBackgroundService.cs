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
    public sealed class DocumentOnePublishUpdateProcessingBackgroundService :
        DocumentPublishUpdateBackgroundService<IDocumentOnePublishUpdateConsumerService,
            DocumentOnePublishUpdateConsumerServiceSettings, DocumentPublishUpdateEventMessage>
    {
        public DocumentOnePublishUpdateProcessingBackgroundService(
            IServiceProvider provider,
            IOptions<DocumentOnePublishUpdateConsumerServiceSettings> settings,
            ILogger<DocumentPublishUpdateBackgroundService<IDocumentOnePublishUpdateConsumerService,
                DocumentOnePublishUpdateConsumerServiceSettings, DocumentPublishUpdateEventMessage>> logger)
            : base(provider, settings, logger)
        {
        }
    }
}
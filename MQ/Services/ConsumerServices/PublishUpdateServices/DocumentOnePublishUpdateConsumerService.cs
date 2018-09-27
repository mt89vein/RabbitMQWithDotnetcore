using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishUpdateServices;
using MQ.Configuration.Consumers.PublishUpdateSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services
{
    public class DocumentOnePublishUpdateConsumerService : ConsumerService<DocumentOne>, IDocumentOnePublishUpdateConsumerService
    {
        public DocumentOnePublishUpdateConsumerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentOnePublishUpdateConsumerServiceSettings> settings,
            ILogger<DocumentOnePublishUpdateConsumerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
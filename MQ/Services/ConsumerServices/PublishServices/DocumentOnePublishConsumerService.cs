using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishServices;
using MQ.Configuration.Consumers.PublishSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services
{
    public class DocumentOnePublishConsumerService : ConsumerService<DocumentOne>, IDocumentOnePublishConsumerService
    {
        public DocumentOnePublishConsumerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentOnePublishConsumerServiceSettings> settings,
            ILogger<DocumentOnePublishConsumerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
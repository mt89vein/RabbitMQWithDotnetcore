using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishUpdateServices;
using MQ.Configuration.Consumers.PublishUpdateSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services
{
    public class DocumentTwoPublishUpdateConsumerService : ConsumerService<DocumentTwo>, IDocumentTwoPublishUpdateConsumerService
    {
        public DocumentTwoPublishUpdateConsumerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentTwoPublishUpdateConsumerServiceSettings> settings,
            ILogger<DocumentTwoPublishUpdateConsumerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
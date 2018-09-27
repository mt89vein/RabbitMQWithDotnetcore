using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishServices;
using MQ.Configuration.Consumers.PublishSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services
{
    public class DocumentTwoPublishConsumerService : ConsumerService<DocumentTwo>, IDocumentTwoPublishConsumerService
    {
        public DocumentTwoPublishConsumerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentTwoPublishConsumerServiceSettings> settings,
            ILogger<DocumentTwoPublishConsumerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }


}
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.QueueServices;
using MQ.Configuration;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishQueueService : QueueService, IDocumentPublishQueueService
    {
        public DocumentPublishQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishQueueSettings> settings, ILogger<DocumentPublishQueueService> logger) 
            : base(persistentConnection,settings, logger)
        {
        }
    }
}
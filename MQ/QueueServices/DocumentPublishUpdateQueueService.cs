using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.QueueServices;
using MQ.Configuration;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishUpdateQueueService : QueueService, IDocumentPublishUpdateQueueService
    {
        public DocumentPublishUpdateQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishUpdateQueueSettings> settings, ILogger<DocumentPublishQueueService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
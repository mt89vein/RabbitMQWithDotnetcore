using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.QueueServices;
using MQ.Configuration;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishResultQueueService : QueueService, IDocumentPublishResultQueueService
    {
        public DocumentPublishResultQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishResultQueueSettings> settings, ILogger<DocumentPublishQueueService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
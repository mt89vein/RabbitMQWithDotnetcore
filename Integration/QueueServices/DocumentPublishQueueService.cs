using Infrastructure.EventBus.Abstractions;
using Infrastructure.EventBus.QueueServices;
using Integration.Abstractions.QueueServices;
using Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integration.QueueServices
{
    public class DocumentPublishQueueService : QueueService, IDocumentPublishQueueService
    {
        public DocumentPublishQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishQueueSettings> settings, ILogger<DocumentPublishQueueService> logger) 
            : base(persistentConnection,settings, logger)
        {
        }
    }
}
using Infrastructure.EventBus.Abstractions;
using Infrastructure.EventBus.QueueServices;
using Integration.Abstractions.QueueServices;
using Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integration.QueueServices
{
    public class DocumentPublishResultQueueService : QueueService, IDocumentPublishResultQueueService
    {
        public DocumentPublishResultQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishResultQueueSettings> settings, ILogger<DocumentPublishQueueService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
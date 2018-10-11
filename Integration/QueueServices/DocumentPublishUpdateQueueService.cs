using Infrastructure.EventBus.Abstractions;
using Infrastructure.EventBus.QueueServices;
using Integration.Abstractions.QueueServices;
using Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integration.QueueServices
{
    public class DocumentPublishUpdateQueueService : QueueService, IDocumentPublishUpdateQueueService
    {
        public DocumentPublishUpdateQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishUpdateQueueSettings> settings, ILogger<DocumentPublishQueueService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
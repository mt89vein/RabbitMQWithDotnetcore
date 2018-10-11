using Infrastructure.EventBus.Abstractions;
using Infrastructure.EventBus.QueueServices;
using Integration.Abstractions.QueueServices;
using Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integration.QueueServices
{
    public class DocumentPublishPublishCancelQueueService : QueueService, IDocumentPublishCancelQueueService
    {
        public DocumentPublishPublishCancelQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishCancelQueueSettings> settings, ILogger<DocumentPublishPublishCancelQueueService> logger) 
            : base(persistentConnection,settings, logger)
        {
        }
    }
}
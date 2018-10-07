using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.QueueServices;
using MQ.Configuration;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishPublishCancelQueueService : QueueService, IDocumentPublishCancelQueueService
    {
        public DocumentPublishPublishCancelQueueService(IPersistentConnection persistentConnection, IOptions<DocumentPublishCancelQueueSettings> settings, ILogger<DocumentPublishPublishCancelQueueService> logger) 
            : base(persistentConnection,settings, logger)
        {
        }
    }
}
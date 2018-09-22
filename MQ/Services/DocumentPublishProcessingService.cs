using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishProcessingService : BaseConsumerService, IDocumentPublishProcessingService
    {
        public DocumentPublishProcessingService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentPublishQueueSettings> settings,
            ILogger<DocumentPublishProcessingService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
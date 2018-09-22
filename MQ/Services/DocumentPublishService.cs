using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishService : BaseProducerService, IDocumentPublishService
    {
        public DocumentPublishService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentPublishQueueSettings> settings,
            ILogger<DocumentPublishService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
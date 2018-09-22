using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishUpdateService : BaseProducerService, IDocumentPublishUpdateService
    {
        public DocumentPublishUpdateService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
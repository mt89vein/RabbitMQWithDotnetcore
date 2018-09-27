using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Producers.PublishServices;
using MQ.Configuration.Producers.PublishSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services
{
    public class DocumentOnePublishProducerService : ProducerService<DocumentOne>, IDocumentOnePublishProducerService
    {
        public DocumentOnePublishProducerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentOnePublishProducerServiceSettings> settings,
            ILogger<DocumentOnePublishProducerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
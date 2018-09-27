using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Producers.PublishServices;
using MQ.Configuration.Producers.PublishSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services.ProducerServices.PublishServices
{
    public class DocumentTwoPublishProducerService : ProducerService<DocumentTwo>, IDocumentTwoPublishProducerService
    {
        public DocumentTwoPublishProducerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentTwoPublishProducerServiceSettings> settings,
            ILogger<DocumentTwoPublishProducerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
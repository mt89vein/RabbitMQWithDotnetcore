using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Producers.UpdateServices;
using MQ.Configuration.Producers.PublishUpdateSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services.ProducerServices.PublishUpdateServices
{
    public class DocumentOnePublishUpdateProducerService : ProducerService<DocumentOne>,
        IDocumentOnePublishUpdateProducerService
    {
        public DocumentOnePublishUpdateProducerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentOnePublishUpdateProducerServiceSettings> settings,
            ILogger<DocumentOnePublishUpdateProducerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
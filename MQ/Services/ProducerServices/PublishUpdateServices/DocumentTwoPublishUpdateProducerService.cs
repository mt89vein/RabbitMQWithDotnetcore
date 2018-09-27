using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Producers.UpdateServices;
using MQ.Configuration.Producers.PublishUpdateSettings;
using MQ.PersistentConnection;
using MQ.Services.Base;

namespace MQ.Services.ProducerServices.PublishUpdateServices
{
    public class DocumentTwoPublishUpdateProducerService : ProducerService<DocumentTwo>,
        IDocumentTwoPublishUpdateProducerService
    {
        public DocumentTwoPublishUpdateProducerService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentTwoPublishUpdateProducerServiceSettings> settings,
            ILogger<DocumentTwoPublishUpdateProducerService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
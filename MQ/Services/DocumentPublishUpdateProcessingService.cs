using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;

namespace MQ.Services
{
    public class DocumentPublishUpdateProcessingService : BaseConsumerService, IDocumentPublishUpdateProcessingService
    {
        public DocumentPublishUpdateProcessingService(IOptions<DocumentPublishUpdateQueueSettings> settings, ILogger<DocumentPublishUpdateProcessingService> logger) 
            : base(settings, logger)
        {
        }
    }
}

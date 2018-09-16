using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;

namespace MQ.Services
{
    public class DocumentPublishProcessingService : BaseConsumerService, IDocumentPublishProcessingService
    {
        public DocumentPublishProcessingService(IOptions<DocumentPublishQueueSettings> settings, ILogger<DocumentPublishProcessingService> logger) 
            : base(settings, logger)
        {
        }
    }
}

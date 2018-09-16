using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;

namespace MQ.Services
{
    public class DocumentPublishService : BaseProducerService, IDocumentPublishService
    {
        public DocumentPublishService(IOptions<DocumentPublishQueueSettings> settings, ILogger<DocumentPublishService> logger) 
            : base(settings, logger)
        {
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;

namespace MQ.Services
{
    public class DocumentPublishUpdateService : BaseProducerService, IDocumentPublishUpdateService
    {
        public DocumentPublishUpdateService(IOptions<DocumentPublishUpdateQueueSettings> settings, ILogger<DocumentPublishUpdateService> logger) 
            : base(settings, logger)
        {
        }
    }
}

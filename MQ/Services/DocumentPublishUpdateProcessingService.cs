using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Base;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.PersistentConnection;

namespace MQ.Services
{
    public class DocumentPublishUpdateProcessingService : BaseConsumerService, IDocumentPublishUpdateProcessingService
    {
        public DocumentPublishUpdateProcessingService(
            IPersistentConnection persistentConnection,
            IOptions<DocumentPublishUpdateQueueSettings> settings,
            ILogger<DocumentPublishUpdateProcessingService> logger)
            : base(persistentConnection, settings, logger)
        {
        }
    }
}
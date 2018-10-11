using Infrastructure.EventBus.Configuration;
using Microsoft.Extensions.Options;

namespace Integration.Configuration
{
    public class DocumentPublishResultQueueSettings : QueueSettings
    {
        public DocumentPublishResultQueueSettings () { }

        public DocumentPublishResultQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}
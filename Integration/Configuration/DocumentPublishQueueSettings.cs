using Infrastructure.EventBus.Configuration;
using Microsoft.Extensions.Options;

namespace Integration.Configuration
{
    public class DocumentPublishQueueSettings : QueueSettings
    {
        public DocumentPublishQueueSettings () { }

        public DocumentPublishQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}

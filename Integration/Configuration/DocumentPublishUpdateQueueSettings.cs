using Infrastructure.EventBus.Configuration;
using Microsoft.Extensions.Options;

namespace Integration.Configuration
{
    public class DocumentPublishUpdateQueueSettings : QueueSettings
    {
        public DocumentPublishUpdateQueueSettings() { }

        public DocumentPublishUpdateQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}
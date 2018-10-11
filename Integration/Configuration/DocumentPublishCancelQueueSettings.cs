using Infrastructure.EventBus.Configuration;
using Microsoft.Extensions.Options;

namespace Integration.Configuration
{
    public class DocumentPublishCancelQueueSettings : QueueSettings
    {
        public DocumentPublishCancelQueueSettings () { }

        public DocumentPublishCancelQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}
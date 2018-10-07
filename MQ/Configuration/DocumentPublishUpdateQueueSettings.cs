using Microsoft.Extensions.Options;
using MQ.Configuration.Base;

namespace MQ.Configuration
{
    public class DocumentPublishUpdateQueueSettings : BaseQueueSettings
    {
        public DocumentPublishUpdateQueueSettings() { }

        public DocumentPublishUpdateQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}
using System;
using Microsoft.Extensions.Options;
using MQ.Configuration.Base;

namespace MQ.Configuration
{
    public class DocumentPublishQueueSettings : BaseQueueSettings
    {
        public DocumentPublishQueueSettings () { }

        public DocumentPublishQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}

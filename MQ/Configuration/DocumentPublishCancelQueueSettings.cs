using System;
using Microsoft.Extensions.Options;
using MQ.Configuration.Base;

namespace MQ.Configuration
{
    public class DocumentPublishCancelQueueSettings : BaseQueueSettings
    {
        public DocumentPublishCancelQueueSettings () { }

        public DocumentPublishCancelQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}
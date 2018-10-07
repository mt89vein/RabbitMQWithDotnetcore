using Microsoft.Extensions.Options;
using MQ.Configuration.Base;
using System;

namespace MQ.Configuration
{
    public class DocumentPublishResultQueueSettings : BaseQueueSettings
    {
        public DocumentPublishResultQueueSettings () { }

        public DocumentPublishResultQueueSettings(IOptions<ConnectionStrings> connectionStrings) : base(connectionStrings)
        {
        }
    }
}
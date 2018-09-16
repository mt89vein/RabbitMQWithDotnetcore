using System;

namespace MQ.Interfaces
{
    public interface IBaseConsumerService
    {
        void MarkAsCancelled(ulong deliveryTag);

        void MarkAsProcessed(ulong deliveryTag);

        void ProcessQueue(Action<string, ulong> onDequeue, Action<Exception, ulong> onError);
    }
}
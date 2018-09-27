using System;
using System.Threading.Tasks;

namespace MQ.Abstractions.Base
{
    public interface IConsumerService
    {
        void ReEnqueue(ulong deliveryTag);

        void MarkAsProcessed(ulong deliveryTag);

        void MarkAsCancelled(ulong deliveryTag);

        void ProcessQueue<T>(Func<T, ulong, bool> onDequeue, Action<Exception, ulong> onError)
            where T : EventMessage;

        void ProcessQueue<T>(Func<T, ulong, Task<bool>> onDequeue, Action<Exception, ulong> onError)
            where T : EventMessage;
    }
}
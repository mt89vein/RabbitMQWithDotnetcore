using System;
using System.Threading.Tasks;

namespace MQ.Interfaces
{
    public interface IBaseConsumerService
    {
        void ReEnqueue(ulong deliveryTag);

        void MarkAsProcessed(ulong deliveryTag);

        void MarkAsCancelled(ulong deliveryTag);

        void ProcessQueue(Func<string, ulong, bool> onDequeue, Action<Exception, ulong> onError);
        void ProcessQueue(Func<string, ulong, Task<bool>> onDequeue, Action<Exception, ulong> onError);
    }
}
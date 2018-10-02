using System;
using System.Threading.Tasks;

namespace MQ.Abstractions.Base
{
    public interface IConsumerService
    {
        void ProcessQueue<T>(Func<T, Task<bool>> onDequeue, Action<Exception, T> onError)
            where T : EventMessage;
    }
}
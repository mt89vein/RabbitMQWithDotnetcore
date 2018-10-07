using System;
using System.Threading.Tasks;

namespace MQ.Abstractions.Base
{
    public interface IEventBus
    {
        void PublishMessage<T>(T message)
            where T: EventMessage;

        void ProcessQueue<T>(Func<T, Task<bool>> onDequeue, Action<Exception, T> onError)
            where T : EventMessage;
    }
}
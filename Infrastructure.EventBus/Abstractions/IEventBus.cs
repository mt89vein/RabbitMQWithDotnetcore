using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.EventBus.Abstractions
{
    public interface IEventBus
    {
        void PublishMessage<T>(T message)
            where T: EventMessage;

        void ProcessQueue<T>(Func<T, CancellationToken, Task<bool>> onDequeue, Action<Exception, T> onError, CancellationToken cancellationToken)
            where T : EventMessage;
    }
}
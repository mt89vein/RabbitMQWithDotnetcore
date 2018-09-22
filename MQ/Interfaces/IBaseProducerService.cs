using MQ.Interfaces;
using MQ.Interfaces.Messages;

namespace MQ.Interfaces
{
    public interface IBaseProducerService
    {
        ulong PublishMessage(IMessage message);
        void RemoveMessage(ulong deliveryTag);
    }
}
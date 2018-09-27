namespace MQ.Abstractions.Base
{
    public interface IProducerService
    {
        ulong PublishMessage(EventMessage eventMessage);

        void RemoveMessage(ulong deliveryTag);
    }
}
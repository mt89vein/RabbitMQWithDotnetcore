using System;
using RabbitMQ.Client;

namespace MQ.PersistentConnection
{
    public interface IPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        IModel CreateModel();

        bool TryConnect();
    }
}
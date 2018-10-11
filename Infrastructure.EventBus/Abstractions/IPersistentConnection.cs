using System;
using RabbitMQ.Client;

namespace Infrastructure.EventBus.Abstractions
{
    public interface IPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        IModel CreateModel();

        bool TryConnect();
    }
}
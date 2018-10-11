using Infrastructure.EventBus.Abstractions;

namespace Integration.EventMessages
{
    /// <summary>
    /// Класс для передачи сообщений в Rabbit MQ в очередь отмены публикации документа
    /// </summary>
    public class DocumentPublishCancelEventMessage : EventMessage
    {
    }
}
using MQ.Abstractions.Base;

namespace MQ.Messages
{
    /// <summary>
    /// Класс для передачи сообщений в Rabbit MQ в очередь отмены публикации документа
    /// </summary>
    public class DocumentPublishCancelEventMessage : EventMessage
    {
    }
}
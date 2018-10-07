using MQ.Abstractions.Base;

namespace MQ.Messages
{
    /// <summary>
    /// Интерфейс для передачи сообщений в Rabbit MQ в очередь обновления информации о публикации документа
    /// </summary>
    public class DocumentPublishUpdateEventMessage : EventMessage
    {
        /// <summary>
        /// Ссылка на идентификатор документа в публикуемой системе
        /// </summary>
        public string RefId { get; set; }
    }
}

namespace MQ.Abstractions.Messages
{
    /// <summary>
    /// Интерфейс для передачи сообщений в Rabbit MQ в очередь обновления информации о публикации документа
    /// </summary>
    public class DocumentPublishUpdateEventMessage : DocumentPublishEventMessage
    {
        /// <summary>
        /// Ссылка на идентификатор документа в публикуемой системе
        /// </summary>
        public string RefId { get; set; }
    }
}

namespace MQ.Interfaces.Messages
{
    /// <summary>
    /// Интерфейс для передачи сообщений в Rabbit MQ в очередь обновления информации о публикации документа
    /// </summary>
    public interface IPublishUpdateMessage : IMessage
    {
        /// <summary>
        /// Ссылка на идентификатор документа в публикуемой системе
        /// </summary>
        string RefId { get; set; }
    }
}

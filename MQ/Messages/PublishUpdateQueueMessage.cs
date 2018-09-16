using System;
using MQ.Interfaces.Messages;

namespace MQ.Messages
{
    /// <summary>
    /// Класс для передачи сообщений в Rabbit MQ в очередь обновления информации о публикации документа
    /// </summary>
    public class PublishUpdateQueueMessage : IPublishUpdateMessage
    {
        /// <summary>
        /// Время постановки в очередь
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Количество возникших повторов
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Ссылка на идентификатор документа в публикуемой системе
        /// </summary>
        public string RefId { get; set; }
    }
}
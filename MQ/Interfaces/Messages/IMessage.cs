using System;

namespace MQ.Interfaces.Messages
{
    /// <summary>
    /// Базовый интерфейс для передачи сообщений в Rabbit MQ
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Время постановки в очередь
        /// </summary>
        DateTime TimeStamp { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        int UserId { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Количество возникших повторов
        /// </summary>
        int RetryCount { get; set; }
    }
}

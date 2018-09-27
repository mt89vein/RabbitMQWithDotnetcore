using System;

namespace MQ.Abstractions.Base
{
    /// <summary>
    /// Базовый интерфейс для передачи сообщений в Rabbit MQ
    /// </summary>
    public abstract class EventMessage
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
    }
}

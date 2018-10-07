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
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }
    }
}

using Domain;
using Infrastructure.EventBus.Abstractions;

namespace Integration.EventMessages
{
    /// <summary>
    /// Класс для передачи сообщений в Rabbit MQ в очередь результата публикации документа
    /// </summary>
    public class DocumentPublishResultEventMessage : EventMessage
    {
        /// <summary>
        /// Результат публикации
        /// </summary>
        public PublicationResultType ResultType { get; set; }

        /// <summary>
        /// Идентификатор загрузки
        /// </summary>
        public long? LoadId { get; set; }
    }
}
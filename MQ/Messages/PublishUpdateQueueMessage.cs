using System;
using Domain;
using MQ.Interfaces.Messages;
using Newtonsoft.Json;

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
        /// Ссылка на идентификатор документа в публикуемой системе
        /// </summary>
        public string RefId { get; set; }

        /// <summary>
        /// Идентификатор документа
        /// </summary>
        public RevisionIdentity RevisionIdentity { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Данные, необходимые для обработки запроса
        /// </summary>
        [JsonConverter(typeof(ConcreteTypeConverter<SomeDocumentPublishUserData>))]
        public IUserData UserData { get; set; }
    }
}
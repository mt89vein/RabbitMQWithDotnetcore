﻿using Domain;
using MQ.Abstractions.Base;
using MQ.Messages;

namespace MQ.Abstractions.Messages
{
    /// <summary>
    /// Интерфейс для передачи сообщений в Rabbit MQ в очередь публикации документа
    /// </summary>
    public class DocumentPublishEventMessage : EventMessage
    {
        /// <summary>
        /// Идентификатор документа
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Ревизия документа
        /// </summary>
        public int DocumentRevision { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Данные, необходимые для обработки запроса
        /// </summary>
        public UserInputData UserInputData { get; set; }
    }
}

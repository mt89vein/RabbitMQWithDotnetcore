using System;
using System.Collections.Generic;
using Domain;
using Integration.EventMessages;
using Newtonsoft.Json;

namespace Integration.Models
{
    /// <summary>
    /// Задача публикации документа
    /// </summary>
    public class PublishDocumentTask
    {
        public PublishDocumentTask()
        {
            PublishDocumentTaskAttempts = new List<PublishDocumentTaskAttempt>();
            State = PublishState.None;
            RefId = null;
            UpdatedAt = null;
            LoadId = null;
        }

        public PublishDocumentTask(DocumentPublishEventMessage message)
        {
            PublishDocumentTaskAttempts = new List<PublishDocumentTaskAttempt>();
            State = PublishState.None;
            RefId = null;
            UpdatedAt = null;
            LoadId = null;
            Id = message.Id;
            DocumentId = message.DocumentId;
            DocumentRevision = message.DocumentRevision;
            DocumentType = message.DocumentType;
            UserId = message.UserId;
            State = PublishState.None;
            CreatedAt = message.CreatedAt;
            OrganizationId = message.OrganizationId;
            Payload = JsonConvert.SerializeObject(message.AdditionalPublicationData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        /// <summary>
        /// Идентификатор задачи
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Идентификатор организации
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Идентификатор документа
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Ревизия документа
        /// </summary>
        public int DocumentRevision { get; set; }

        /// <summary>
        /// Данные введенные пользователем в формате JSON
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// Дата постановки задачи
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата последнего обновления задачи
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Идентификатор пакета
        /// </summary>
        public string RefId { get; set; }

        /// <summary>
        /// Идентификатор внешней системы
        /// </summary>
        public long? LoadId { get; set; }

        /// <summary>
        /// Состояние публикации
        /// </summary>
        public PublishState State { get; set; }

        /// <summary>
        /// Доставлен результат о публикации
        /// </summary>
        public bool IsDelivered { get; set; }

        /// <summary>
        /// В одной из попыток публикации имеется внутреняя ошибка в ЕИС
        /// </summary>
        public bool HasEisExceptions { get; set; }

        /// <summary>
        /// Завершена ли работа над задачей
        /// </summary>
        public bool IsFinished =>
            State == PublishState.Canceled ||
            State == PublishState.Published ||
            State == PublishState.Failed ||
            State == PublishState.XmlValidationError;

        /// <summary>
        /// Список попыток публикации документа
        /// </summary>
        [JsonIgnore]
        public List<PublishDocumentTaskAttempt> PublishDocumentTaskAttempts { get; set; }
    }
}
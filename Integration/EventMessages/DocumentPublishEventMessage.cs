using Domain;
using Domain.Abstractions;
using Infrastructure.EventBus.Abstractions;
using Integration.Models;
using Newtonsoft.Json;

namespace Integration.EventMessages
{
    /// <summary>
    /// Класс для передачи сообщений в Rabbit MQ в очередь публикации документа
    /// </summary>
    public class DocumentPublishEventMessage : EventMessage
    {
        public DocumentPublishEventMessage()
        {
        }

        public DocumentPublishEventMessage(PublishDocumentTask publishDocumentTask)
        {
            Id = publishDocumentTask.Id;
            AdditionalPublicationData = (AdditionalPublicationData) JsonConvert.DeserializeObject(publishDocumentTask.Payload, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            DocumentId = publishDocumentTask.DocumentId;
            DocumentRevision = publishDocumentTask.DocumentRevision;
            DocumentType = publishDocumentTask.DocumentType;
            OrganizationId = publishDocumentTask.OrganizationId;
            UserId = publishDocumentTask.UserId;
        }

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
        public AdditionalPublicationData AdditionalPublicationData { get; set; }

        /// <summary>
        /// Организация, которой принадлежит документ
        /// </summary>
        public int OrganizationId { get; set; }
    }
}

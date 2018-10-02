using System;
using Domain;
using MQ.Abstractions.Messages;
using MQ.Messages;
using MQ.Models;
using Newtonsoft.Json;

namespace Client.ViewModels
{
    public class PublishTaskViewModel<TUserInputData>
        where TUserInputData : UserInputData
    {
        public Guid Id { get; set; }

        public int DocumentId { get; set; }

        public int DocumentRevision { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }

        public int OrganizationId { get; set; }

        public DocumentType DocumentType { get; set; }

        public TUserInputData UserInputData { get; set; }

        public PublishDocumentTask GetPublishDocumentTask()
        {
            return new PublishDocumentTask
            {
                Id = Id,
                DocumentType = DocumentType,
                CreatedAt = CreatedAt,
                DocumentId = DocumentId,
                DocumentRevision = DocumentRevision,
                State = PublishState.None,
                OrganizationId = OrganizationId,
                Payload = JsonConvert.SerializeObject(UserInputData),
                UserId = UserId
            };
        }

        public DocumentPublishEventMessage GetDocumentPublishEventMessage()
        {
            return new DocumentPublishEventMessage
            {
                Id = Id,
                DocumentId = DocumentId,
                DocumentRevision = DocumentRevision,
                DocumentType = DocumentType,
                CreatedAt = CreatedAt,
                UserId = UserId,
                UserInputData = UserInputData
            };
        }
    }
}
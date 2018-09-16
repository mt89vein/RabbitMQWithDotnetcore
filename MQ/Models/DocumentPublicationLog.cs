using System;
using Domain;

namespace MQ.Models
{
    public class DocumentPublicationLog
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }

        public int OrganizationId { get; set; }

        public DocumentType DocumentType { get; set; }

        public RevisionIdentity RevisionIdentity { get; set; }

        public string Request { get; set; }

        public string Response { get; set; }

        public DateTime RequestStartedAt { get; set; }

        public DateTime RequestEndedAt { get; set; }

        public string Result { get; set; }

        public string RefId { get; set; }

        public long? LoadId { get; set; }

        public PublishState PublishState { get; set; }

        public ulong DeliveryTag { get; set; }
    }

    public enum PublishState
    {
        Awaiting,
        Processing,
        Published,
        Canceled
    }
}
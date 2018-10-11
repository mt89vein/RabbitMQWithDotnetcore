using System;
using Domain;
using Integration.Models;

namespace Integration.Repositories
{
    public class PublishDocumentTaskFilter : PagedFilter
    {
        public Guid? Id { get; set; }

        public DocumentType[] DocumentTypes { get; set; }

        public int? DocumentId { get; set; }

        public int? DocumentRevision { get; set; }

        public DateTime[] Enqueued { get; set; }

        public bool? IsDelivered { get; set; }

        public bool? HasEisExceptions { get; set; }

        public int? OrganizationId { get; set; }

        public int? UserId { get; set; }

        public PublishState[] States { get; set; }
    }
}
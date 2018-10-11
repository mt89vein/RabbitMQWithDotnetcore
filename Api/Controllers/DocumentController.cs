using System;
using Domain;
using Domain.AdditionalPublicationData;
using Integration.Abstractions.QueueServices;
using Integration.EventMessages;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentPublishQueueService _documentPublishQueueService;

        public DocumentController(IDocumentPublishQueueService documentPublishQueueService)
        {
            _documentPublishQueueService = documentPublishQueueService ??
                                           throw new ArgumentNullException(nameof(documentPublishQueueService));
        }

        [HttpPost]
        [Route("Many")]
        public virtual ActionResult Publish()
        {
            for (var i = 0; i < 1000; i++)
            {
                var message = new DocumentPublishEventMessage
                {
                    DocumentId = i,
                    DocumentRevision = i + 155,
                    DocumentType = DocumentType.One,
                    Id = Guid.NewGuid(),
                    OrganizationId = i,
                    UserId = i,
                    AdditionalPublicationData = new DocumentOnePublishAdditionalPublicationData
                    {
                        Password = "test",
                        Login = "1",
                        RegistryNumber = "4"
                    }
                };

                _documentPublishQueueService.PublishMessage(message);
            }

            return Ok();
        }

        [HttpPost]
        [Route("Single")]
        public virtual ActionResult PublishSingle(Guid guid)
        {
            var message = new DocumentPublishEventMessage
            {
                DocumentId = 512,
                DocumentRevision = 1155,
                DocumentType = DocumentType.One,
                Id = guid,
                OrganizationId = 21,
                UserId = 5,
                AdditionalPublicationData = new DocumentOnePublishAdditionalPublicationData
                {
                    Password = "test",
                    Login = "1",
                    RegistryNumber = "4"
                }
            };

            _documentPublishQueueService.PublishMessage(message);

            return Ok();
        }
    }
}
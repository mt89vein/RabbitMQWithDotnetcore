using System;
using System.Threading;
using Client.ViewModels;
using Domain;
using Microsoft.AspNetCore.Mvc;
using MQ.Abstractions.Producers.PublishServices;
using MQ.Messages;
using MQ.Models;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentOneController : DocumentPublishController<IDocumentOnePublishProducerService,
        DocumentOnePublishUserInputData>
    {
        private readonly IDocumentOnePublishProducerService _documentOnePublishProducerService;
        public DocumentOneController(IDocumentOnePublishProducerService documentPublishProducerService)
            : base(documentPublishProducerService)
        {
            _documentOnePublishProducerService = documentPublishProducerService ??
                                                 throw new ArgumentNullException(nameof(documentPublishProducerService));
        }

        [HttpPost]
        [Route("Many")]
        public virtual ActionResult Publish([FromServices] PublishDocumentTaskContext context)
        {
            for (int i = 0; i < 1000; i++)
            {
                var vm = new PublishTaskViewModel<DocumentOnePublishUserInputData>
                {
                    DocumentId = i,
                    DocumentRevision = i + 155,
                    DocumentType = DocumentType.One,
                    Id = Guid.NewGuid(),
                    OrganizationId = i,
                    UserId = i,
                    UserInputData = new DocumentOnePublishUserInputData
                    {
                        Password = "test",
                        Login = "1",
                        RegistryNumber = "4"
                    }
                };

                var publishDocumentTask = vm.GetPublishDocumentTask();
                var message = vm.GetDocumentPublishEventMessage();
                _documentOnePublishProducerService.PublishMessage(message);
                context.PublishDocumentTasks.Add(publishDocumentTask);
            }
            
            context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        [Route("Single")]
        public virtual ActionResult PublishSingle(Guid guid)
        {
            var vm = new PublishTaskViewModel<DocumentOnePublishUserInputData>
            {
                DocumentId = 512,
                DocumentRevision = 1155,
                DocumentType = DocumentType.One,
                Id = guid,
                OrganizationId = 21,
                UserId = 5,
                UserInputData = new DocumentOnePublishUserInputData
                {
                    Password = "test",
                    Login = "1",
                    RegistryNumber = "4"
                }
            };

            _documentOnePublishProducerService.PublishMessage(vm.GetDocumentPublishEventMessage());

            return Ok();
        }
    }
}
using System;
using System.Threading;
using Client.ViewModels;
using Domain;
using Microsoft.AspNetCore.Mvc;
using MQ.Abstractions.Base;
using MQ.Messages;
using MQ.Models;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class DocumentPublishController<TDocumentPublishProducerService, TUserInputData> : ControllerBase
        where TDocumentPublishProducerService : class, IProducerService
        where TUserInputData : UserInputData, new()
    {
        private readonly TDocumentPublishProducerService _documentPublishProducerService;

        protected DocumentPublishController(TDocumentPublishProducerService documentPublishProducerService)
        {
            _documentPublishProducerService = documentPublishProducerService ??
                                                 throw new ArgumentNullException(nameof(documentPublishProducerService));
        }

        [Route("")]
        [HttpPost]
        public virtual ActionResult Publish([FromBody] PublishTaskViewModel<TUserInputData> viewModel, [FromServices] PublishDocumentTaskContext context)
        {
            var publishDocumentTask = viewModel.GetPublishDocumentTask();
            var message = viewModel.GetDocumentPublishEventMessage();
            context.PublishDocumentTasks.Add(publishDocumentTask);
            context.SaveChanges();
            _documentPublishProducerService.PublishMessage(message);

            return Ok();
        }
    }
}
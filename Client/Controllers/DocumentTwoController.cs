using Microsoft.AspNetCore.Mvc;
using MQ.Abstractions.Producers.PublishServices;
using MQ.Messages;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentTwoController : DocumentPublishController<IDocumentTwoPublishProducerService,
        DocumentTwoPublishUserInputData>
    {
        public DocumentTwoController(IDocumentTwoPublishProducerService documentPublishProducerService)
            : base(documentPublishProducerService)
        {
        }
    }
}
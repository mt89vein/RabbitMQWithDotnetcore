using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MQ.Abstractions.Repositories;
using MQ.Repositories;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishDocumentTaskController : ControllerBase
    {
        private readonly IPublishDocumentTaskRepository _publishDocumentTaskRepository;

        public PublishDocumentTaskController(IPublishDocumentTaskRepository publishDocumentTaskRepository)
        {
            _publishDocumentTaskRepository = publishDocumentTaskRepository ??
                                             throw new ArgumentNullException(nameof(publishDocumentTaskRepository));
        }

        [HttpGet]
        [Route("GetTasksByFilter")]
        public async Task<IActionResult> GetTasksByFilter([FromQuery] PublishDocumentTaskFilter filter)
        {
            var filteredTasks = await _publishDocumentTaskRepository.GetTasksByFilterAsync(filter);

            return Ok(filteredTasks);
        }

        [HttpGet]
        [Route("GetAttemptsByTaskId/{id:guid}")]
        public async Task<IActionResult> GetAttemptsByTaskId(Guid id)
        {
            var attempts = await _publishDocumentTaskRepository.GetAttemtpsAsync(id);

            return Ok(attempts);
        }
    }
}
using System;
using System.Threading.Tasks;
using Integration.Abstractions;
using Integration.Abstractions.QueueServices;
using Integration.EventMessages;
using Integration.Models;
using Integration.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
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
        public async Task<IActionResult> GetTasksByFilter([FromQuery] PublishDocumentTaskFilter filter)
        {
            var filteredTasks = await _publishDocumentTaskRepository.GetTasksByFilterAsync(filter);

            return Ok(filteredTasks);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAttemptsByTaskId(Guid id)
        {
            var attempts = await _publishDocumentTaskRepository.GetAttemtpsAsync(id);

            return Ok(attempts);
        }

        [HttpPost]
        public async Task<IActionResult> ReEnqueueTaskById([FromBody] Guid id, [FromServices] IDocumentPublishQueueService documentPublishQueueService)
        {
            var task = await _publishDocumentTaskRepository.GetAsync(id);
            if (task == null)
            {
                return NotFound("Задача не найдена");
            }

            if (!task.IsFinished)
            {
                return UnprocessableEntity("Задача на публикацию еще не завершена");
            }

            if (task.LoadId != null)
            {
                return UnprocessableEntity($"Документ опубликован с идентификатором {task.LoadId}");
            }

            if (String.IsNullOrWhiteSpace(task.Payload))
            {
                return UnprocessableEntity("Нет данных пользователя для повторной отправки задачи");
            }

            DocumentPublishEventMessage documentPublishEventMessage;
            try
            {
                documentPublishEventMessage = new DocumentPublishEventMessage(task);
            }
            catch (Exception e)
            {
                return UnprocessableEntity("Не удалось десериализовать данные пользователя для повторной отправки задачи " + e.Message);
            }

            try
            {
                task.State = PublishState.None;
                task.UpdatedAt = DateTime.Now;
                task.IsDelivered = false;
                task.RefId = null;

                _publishDocumentTaskRepository.Save(task);

                documentPublishQueueService.PublishMessage(documentPublishEventMessage);
            }
            catch (Exception e)
            {
                return UnprocessableEntity($"Возникла ошибка при попытке нарпавить на повторную публикацию {e.Message}");
            }

            return Ok("Задача успешно направлена на повторную публикацию");
        }
    }
}
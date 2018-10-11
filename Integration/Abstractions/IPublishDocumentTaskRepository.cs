using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Integration.Models;
using Integration.Repositories;

namespace Integration.Abstractions
{
    public interface IPublishDocumentTaskRepository
    {
        /// <summary>
        /// Получить задачу на публикацию по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Задача на публикацию</returns>
        Task<PublishDocumentTask> GetAsync(Guid id);

        /// <summary>
        /// Получить попытки публикации по идентификатору задачи
        /// </summary>
        /// <param name="publishDocumentTaskId">Идентификатор задачи</param>
        /// <returns>Попытки публикации для конкретной задаче</returns>
        Task<List<PublishDocumentTaskAttempt>> GetAttemtpsAsync(Guid publishDocumentTaskId);

        /// <summary>
        /// Получить задачи на публикацию по фильтру
        /// </summary>
        /// <returns>Список задач по фильтру</returns>
        Task<DataPage<PublishDocumentTask>> GetTasksByFilterAsync(PublishDocumentTaskFilter filter = null);

        /// <summary>
        /// Сохранить задачу на публикацию по идентификатору
        /// </summary>
        /// <param name="publishDocumentTask"></param>
        void Save(PublishDocumentTask publishDocumentTask);
    }
}
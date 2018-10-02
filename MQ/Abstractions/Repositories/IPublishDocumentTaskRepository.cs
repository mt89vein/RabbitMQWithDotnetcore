using System;
using System.Threading.Tasks;
using Domain;
using MQ.Models;

namespace MQ.Abstractions.Repositories
{
    public interface IPublishDocumentTaskRepository
    {
        /// <summary>
        /// Получить задачу на публикацию по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns>Задача на публикацию</returns>
        Task<PublishDocumentTask> Get(Guid id);

        /// <summary>
        /// Сохранить результат попытки публикации
        /// </summary>
        void SavePublishAttempt(PublishDocumentTask publishDocumentTask, DocumentPublicationInfo documentPublicationInfo);
    }
}
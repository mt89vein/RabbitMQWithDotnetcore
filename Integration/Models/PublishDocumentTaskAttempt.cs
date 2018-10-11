using System;
using Domain;

namespace Integration.Models
{
    /// <summary>
    /// Попытка публикации по задаче
    /// </summary>
    public class PublishDocumentTaskAttempt
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Запрос
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// Ответ
        /// </summary>
        public string Response { get; set; }
        
        /// <summary>
        /// Содержит одну или несколько внутренних ошибок в ЕИС
        /// </summary>
        /// <remarks>Смотрим результат ответа (HTTP status, и тело XML на наличие колстека и т.д)</remarks>
        public bool HasEisExceptions { get; set; }

        /// <summary>
        /// Дата и время попытки
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Идентификатор задачи публикации документа
        /// </summary>
        public Guid PublishDocumentTaskId { get; set; }

        /// <summary>
        /// Задача публикации документа
        /// </summary>
        public PublishDocumentTask PublishDocumentTask { get; set; }

        /// <summary>
        /// Результат запроса
        /// </summary>
        public PublicationResultType Result { get; set; }
    }
}
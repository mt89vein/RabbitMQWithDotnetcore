using Domain;
using MQ.Messages;

namespace MQ.Interfaces.Messages
{
    /// <summary>
    /// Интерфейс для передачи сообщений в Rabbit MQ в очередь публикации документа
    /// </summary>
    public interface IPublishMessage : IMessage
    {
        /// <summary>
        /// Идентификатор документа
        /// </summary>
        RevisionIdentity RevisionIdentity { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        DocumentType DocumentType { get; set; }

        /// <summary>
        /// Данные, необходимые для обработки запроса
        /// </summary>
        IUserData UserData { get; set; }
    }
}

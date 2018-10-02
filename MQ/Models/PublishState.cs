namespace MQ.Models
{
    public enum PublishState
    {
        /// <summary>
        /// Еще не было попыток публиковать
        /// </summary>
        None,

        /// <summary>
        /// Документ ушел и находится в обработке
        /// </summary>
        Processing,

        /// <summary>
        /// Документ успешно опубликован
        /// </summary>
        Published,

        /// <summary>
        /// Документ не опубликован из-за ошибок в документе
        /// </summary>
        Failed,

        /// <summary>
        /// Ошибка при валидации XML
        /// </summary>
        XmlValidationError,

        /// <summary>
        /// Отменена публикация
        /// </summary>
        Canceled
    }
}
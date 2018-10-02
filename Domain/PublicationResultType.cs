namespace Domain
{
    /// <summary>
    /// Тип результата публикации документа
    /// </summary>
    public enum PublicationResultType
    {
        /// <summary>
        /// Публикация произошла успешно
        /// </summary>
        Success,

        /// <summary>
        /// Сервер вернул сообщение об ошибке
        /// </summary>
        Failed,

        /// <summary>
        /// В обработке
        /// </summary>
        Processing,

        /// <summary>
        /// Ошибка валидации XML
        /// </summary>
        XmlValidationError
    }
}
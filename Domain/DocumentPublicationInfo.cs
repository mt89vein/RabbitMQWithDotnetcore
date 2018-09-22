namespace Domain
{
    public class DocumentPublicationInfo
    {
        public DocumentPublicationInfo(string refId, PublicationResultType resultType, long? loadId)
        {
            RefId = refId;
            ResultType = resultType;
            LoadId = loadId;
        }

        /// <summary>
        /// Идентификатор загруженного пакета
        /// </summary>
        public string RefId { get; }

        /// <summary>
        /// Тип результата
        /// </summary>
        public PublicationResultType ResultType { get; }

        /// <summary>
        /// Идентификатор загрузки
        /// </summary>
        public long? LoadId { get; }
    }
}

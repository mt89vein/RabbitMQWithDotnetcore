using System;

namespace Domain
{
    public class DocumentPublicationInfo
    {
        public DocumentPublicationInfo(string refId, PublicationResultType resultType, long? loadId, string request, string response)
        {
            RefId = refId;
            ResultType = resultType;
            LoadId = loadId;
            Request = request;
            Response = response;
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

        /// <summary>
        /// Запрос
        /// </summary>
        public string Request { get; }

        /// <summary>
        /// Ответ
        /// </summary>
        public string Response { get; }

        /// <summary>
        /// Были ли возвращены ошибки от ЕИС (имеется ввиду внутренние ошибки)
        /// </summary>
        public bool IsHasEisError => TryCheckIsHasEisErrors(Response);

        private static bool TryCheckIsHasEisErrors(string response)
        {
            if (!String.IsNullOrWhiteSpace(response))
            {
                // check response..
            }

            return false;
        }
    }
}

namespace MQ.Configuration.Base
{
    public class DocumentPublishQueueSettings : BaseQueueSettings
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Количество воркеров
        /// </summary>
        public int ConsumersCount { get; set; } = 5;
    }
}

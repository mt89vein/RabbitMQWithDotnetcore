namespace MQ.Configuration
{
    public class DocumentPublishQueueSettings : RabbitMqConnectionSettings
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Количество воркеров
        /// </summary>
        public int ConsumersCount { get; set; } = 5;

        /// <summary>
        /// Максимальное количество попыток разместить документ, в случае возникновения внештатных ситуаций
        /// </summary>
        public int MaxRetryCount { get; set; } = 2;
    }
}

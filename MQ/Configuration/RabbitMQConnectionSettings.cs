namespace MQ.Configuration
{
    public class RabbitMqConnectionSettings
    {
        /// <summary>
        /// Название очереди
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Название обменника
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Название ключа маршрута
        /// </summary>
        public string RoutingKey { get; set; } = "";

        /// <summary>
        /// Тип обменника
        /// </summary>
        public string ExchangeType { get; set; } = "direct";

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        public string HostName { get; set; }

        public int? Port { get; set; }

        public ushort RequestedHeartbeat { get; set; }

        public int? NetworkRecoveryInterval { get; set; }

        public int? RetryConnectionAttempt { get; set; }
    }
}

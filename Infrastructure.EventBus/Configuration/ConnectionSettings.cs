namespace Infrastructure.EventBus.Configuration
{
    public class ConnectionSettings
    {
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

namespace MQ.Messages
{
    public abstract class BaseUserData : IUserData
    {
        public string Login { get; set; }

        public string Password { get; set; }
    }
}
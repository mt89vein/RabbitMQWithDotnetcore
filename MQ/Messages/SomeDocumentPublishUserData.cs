namespace MQ.Messages
{
    public class SomeDocumentPublishUserData : BaseUserData
    {
        public int Version { get; set; }

        public bool IsInitialVersion { get; set; }

        public bool IsPublishToProject { get; set; }

        public string RegistryNumber { get; set; }

        public string LoadId { get; set; }
    }
}
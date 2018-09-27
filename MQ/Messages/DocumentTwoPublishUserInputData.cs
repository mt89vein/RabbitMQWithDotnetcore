namespace MQ.Messages
{
    public class DocumentTwoPublishUserInputData : UserInputData
    {
        public int Version { get; set; }

        public string RegistryNumber { get; set; }

        public string LoadId { get; set; }
    }
}
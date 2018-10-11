namespace Domain.AdditionalPublicationData
{
    public class DocumentTwoPublishUserInputData : Abstractions.AdditionalPublicationData
    {
        public int Version { get; set; }

        public string RegistryNumber { get; set; }

        public string LoadId { get; set; }
    }
}
using Integration.Abstractions;

namespace Integration.IntegrationDocuments
{
    public class ConcreteXmlDocumentTypeOne : IOuterXmlDocument
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}

using System;
using Integration.Abstractions;

namespace Integration.IntegrationDocuments
{
    public class ConcreteXmlDocumentTypeThree : IOuterXmlDocument
    {
        public string TestName { get; set; }

        public DateTime EndTime { get; set; }
    }
}
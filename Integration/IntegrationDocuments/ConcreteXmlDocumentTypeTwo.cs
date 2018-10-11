using System;
using Integration.Abstractions;

namespace Integration.IntegrationDocuments
{
    public class ConcreteXmlDocumentTypeTwo : IOuterXmlDocument
    {
        public string Test { get; set; }

        public DateTime StartTime { get; set; }
    }
}
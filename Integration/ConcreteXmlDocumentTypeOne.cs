using System;
using System.Collections.Generic;
using System.Text;

namespace Integration
{
    public class ConcreteXmlDocumentTypeOne : IOuterXmlDocument
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ConcreteXmlDocumentTypeTwo : IOuterXmlDocument
    {
        public string Test { get; set; }

        public DateTime StartTime { get; set; }
    }
}

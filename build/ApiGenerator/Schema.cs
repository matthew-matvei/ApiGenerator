using System.Collections.Generic;

namespace ApiGenerator
{
    public class Schema
    {
        public string Name { get; set; }
        public IEnumerable<SchemaAction> Actions { get; set; }
    }

    public class SchemaAction
    {
        public string Name { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public IEnumerable<SchemaActionParameter> Parameters { get; set; }
    }

    public class SchemaActionParameter
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
    }
}
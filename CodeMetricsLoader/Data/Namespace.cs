using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Namespace
    {
        public int NamespaceId { get; set; }
        public string Name { get; set; }
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; }
        public virtual List<Type> Types { get; set; }
    }
}

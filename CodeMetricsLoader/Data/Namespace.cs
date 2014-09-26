using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Namespace
    {
        public int NamespaceId { get; set; }
        public string Name { get; set; }
        public int ModuleId { get; set; }        
        public virtual List<Type> Types { get; set; }
        public virtual Metrics Metrics { get; set; }
        public int MetricsId { get; set; }

        public Namespace()
        {
            Types = new List<Type>();
        }
    }
}

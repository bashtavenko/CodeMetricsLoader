using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Type
    {
        public int TypeId { get; set; }
        public string Name { get; set; }
        public int NamespaceId { get; set; }        
        public virtual List<Member> Members { get; set; }
        public virtual Metrics Metrics { get; set; }
        public int MetricsId { get; set; }

        public Type()
        {
            Members = new List<Member>();
        }
    }
}

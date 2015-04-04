using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class DimType
    {
        public int TypeId { get; set; }
        public string Name { get; set; }        
        public virtual List<DimNamespace> Namespaces { get; set; }
        public virtual List<DimMember> Members { get; set; }
        public virtual List<FactMetrics> Metrics { get; set; }

        public DimType()
        {
            Namespaces = new List<DimNamespace>();
            Metrics = new List<FactMetrics>();
        }
    }
}

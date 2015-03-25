using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class DimMember
    {
        public int MemberId { get; set; }
        public string Name { get; set; }                   
        public virtual List<DimType> Types { get; set; }
        public string File { get; set; }
        public int? Line { get; set; }
        public virtual List<FactMetrics> Metrics { get; set; }

        public DimMember()
        {
            Types = new List<DimType>();
        }
    }
}

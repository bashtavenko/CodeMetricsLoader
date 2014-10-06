using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class DimRun
    {
        public int RunId { get; set; }
        public string Tag { get; set; }
        public string Target { get; set; }
        public string Module { get; set; }
        public string ModuleFileVersion { get; set; }
        public string ModuleAssemblyVersion { get; set; }
        public string Namespace { get; set; }
        public string Type { get; set; }
        public string Member { get; set; }
        public virtual List<FactMetrics> Metrics { get; set; }

        public DimRun()
        {
            Metrics = new List<FactMetrics>();
        }
    }
}

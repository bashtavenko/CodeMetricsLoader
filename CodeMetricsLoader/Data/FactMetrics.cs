namespace CodeMetricsLoader.Data
{
    public class FactMetrics
    {
        public int MetricsId { get; set; }

        public int DateId { get; set; }
        public virtual DimDate Date { get; set; }

        public int RunId { get; set; }
        public virtual DimRun Run { get; set; }

        public int MaintainabilityIndex { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int ClassCoupling { get; set; }       

        // Members don't have it
        public int? DepthOfInheritance { get; set; }

        public int LinesOfCode { get; set; }                
    }
}

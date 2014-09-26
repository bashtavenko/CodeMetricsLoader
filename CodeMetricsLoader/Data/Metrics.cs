namespace CodeMetricsLoader.Data
{
    public class Metrics
    {
        public int MetricsId { get; set; }
        public int MaintainabilityIndex { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int ClassCoupling { get; set; }
        public int DepthOfInheritance { get; set; }
        public int LinesOfCode { get; set; }        
    }
}

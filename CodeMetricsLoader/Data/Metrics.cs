namespace CodeMetricsLoader.Data
{
    public class Metrics
    {
        public int MaintainabilityIndex { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int ClassCoupling { get; set; }
        public int DepthOfInheritance { get; set; }
        public int LinesOfCode { get; set; }        
        public int? CodeCoverage { get; set; }        
    }
}

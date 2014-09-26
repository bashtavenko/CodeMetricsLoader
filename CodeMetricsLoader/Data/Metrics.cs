namespace CodeMetricsLoader.Data
{
    public class Metrics
    {
        public int MetricsId { get; set; }
        public int MaintanablityIndex { get; set; }
        public int CyclomaticComplexity { get; set; }
        public int ClassCoupling { get; set; }
        public int DepthOfInheritance { get; set; }
        public int LinesOfCode { get; set; }
        public int DateId { get; set; }
        public virtual Date Date { get; set; }
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; }
        public int NamespaceId { get; set; }
        public virtual Namespace Namespace { get; set; }
        public int TypeId { get; set; }
        public virtual Type Type { get; set; }
        public int MemberId { get; set; }
        public virtual Member Member { get; set; }
    }
}

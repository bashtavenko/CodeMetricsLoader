namespace CodeMetricsLoader.Data
{
    public class Member
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public int Line { get; set; }
        public int TypeId { get; set; }
        public virtual Type Type { get; set; }
    }
}

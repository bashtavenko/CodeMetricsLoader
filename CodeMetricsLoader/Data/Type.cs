using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Type
    {
        public int TypeId { get; set; }
        public string Name { get; set; }
        public int NamespaceId { get; set; }
        public Namespace Namespace { get; set; }
        public virtual List<Member> Members { get; set; }
    }
}

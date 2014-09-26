using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Target
    {
        public int TargetId { get; set; }
        public string Name { get; set; }
        public virtual List<Module> Modules { get; set; }
    }
}

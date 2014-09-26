using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Target
    {
        public int TargetId { get; set; }
        public string Name { get; set; }
        public int DateId { get; set; }
        public virtual Date Date { get; set; }
        public string Tag { get; set; }
        public virtual List<Module> Modules { get; set; }

        public Target()
        {
            Modules = new List<Module>();
        }
    }
}

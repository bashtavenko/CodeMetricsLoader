using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Target
    {        
        public string Name { get; set; }             
        public virtual List<Module> Modules { get; set; }

        public Target()
        {
            Modules = new List<Module>();
        }
    }
}

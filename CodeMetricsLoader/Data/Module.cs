using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Module
    {
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public int TargetId { get; set; }
        public virtual Target Target { get; set; }
        public virtual List<Namespace> Namespaces { get; set; }
    }
}

using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Module
    {     
        public string Name { get; set; }
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }     
        public List<Namespace> Namespaces { get; set; }
        public Metrics Metrics { get; set; }        

        public Module ()
        {
            Namespaces = new List<Namespace>();
        }
    }
}

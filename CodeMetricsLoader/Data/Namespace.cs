using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Namespace
    {     
        public string Name { get; set; }     
        public List<Type> Types { get; set; }
        public Metrics Metrics { get; set; }        

        public Namespace()
        {
            Types = new List<Type>();
        }
    }
}

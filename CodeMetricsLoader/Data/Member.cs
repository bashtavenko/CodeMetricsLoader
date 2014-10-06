using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Member
    {     
        public string Name { get; set; }
        public string File { get; set; }
        public int? Line { get; set; }     
        public Metrics Metrics { get; set; }        
    }
}

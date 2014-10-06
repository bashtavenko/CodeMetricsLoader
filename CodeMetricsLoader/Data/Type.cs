using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Type
    {        
        public string Name { get; set; }     
        public virtual List<Member> Members { get; set; }        

        public Type()
        {
            Members = new List<Member>();
        }
    }
}

using System;
using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Member : Node
    {
        public Metrics Metrics { get; set; }
        public override IList<Node> Children { get { return new List<Node>(); }}
        public override string Key { get { return "Member-" + Name; } }
        
        public override int? Value
        {
            get { return Metrics.CodeCoverage; }
            set { Metrics.CodeCoverage = value; }
        }

        public Member()
        {
            Metrics = new Metrics();
        }
    }
}

using System;
using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class Member : Node
    {
        public Metrics Metrics { get; set; }
        public override IList<Node> Children { get { return new List<Node>(); }}
        public override string Key { get { return "Member-" + Name; } }
        public string File { get; set; }
        public int? Line { get; set; }
        
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

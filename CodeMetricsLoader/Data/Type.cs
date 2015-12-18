using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Type : Node
    {   
        public Metrics Metrics { get; set; }
        public IList<Member> Members { get; set; }
        public override string Key { get; }

        public override int? Value
        {
            get { return Metrics.CodeCoverage; }
            set { Metrics.CodeCoverage = value; }
        }

        //Classes and structs do not support variance
        public override IList<Node> Children => (Members as IEnumerable<Node>).ToList();

        public Type(string name) : base (name)
        {
            Members = new List<Member>();
            Metrics = new Metrics();
            Key = "Type-" + Name;
        }

        public override void AddChild(Node child)
        {
            if (!(child is Member))
            {
                throw new ArgumentException("Must have a Member node");
            }

            Members.Add((Member) child);
        }
    }
}

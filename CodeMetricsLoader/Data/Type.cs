using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Type : Node
    {   
        public Metrics Metrics { get; set; }
        public IList<Member> Members { get; set; }
        public override string Key { get { return "Type-" + Name; } }

        public override int? Value
        {
            get { return Metrics.CodeCoverage; }
            set { Metrics.CodeCoverage = value; }
        }

        public override IList<Node> Children
        {
            get
            {
                //Classes and structs do not support variance
                return (Members as IEnumerable<Node>).ToList();
            }
        }

        public Type()
        {
            Members = new List<Member>();
            Metrics = new Metrics();
        }

        public override void AddChild(Node child)
        {
            if (child == null || child as Member == null)
            {
                throw new ArgumentException("Must have a Member node");
            }

            Members.Add(child as Member);
        }
    }
}

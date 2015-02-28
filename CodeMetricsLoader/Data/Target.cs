using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Target : Node
    {        
        public string FileName { get { return System.IO.Path.GetFileName(Name); } }
        public IList<Module> Modules { get; set; }
        public override string Key { get { return "Target-" + FileName; } }

        public override int? Value { get { return (int?) Modules.Average(s => s.Value); } set {} }

        public override IList<Node> Children
        {
            get
            {
                //Classes and structs do not support variance.
                return (Modules as IEnumerable<Node>).ToList();
            }
        }

        public Target()
        {
            Modules = new List<Module>();
        }

        public override void AddChild(Node child)
        {
            if (child == null || child as Module == null)
            {
                throw new ArgumentException("Must have a Module node");
            }

            Modules.Add(child as Module);
        }
    }
}

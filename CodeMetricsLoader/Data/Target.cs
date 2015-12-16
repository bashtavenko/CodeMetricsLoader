using System;
using System.Collections.Generic;
using System.IO;
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
            if (!(child is Module))
            {
                throw new ArgumentException("Must have a Module node");
            }

            Modules.Add((Module) child);
        }

        // Similar to modules
        public override bool Equals(object obj)
        {
            var otherNode = obj as Target;
            return otherNode != null && DropDllOrExeIfExists(FileName).Equals(DropDllOrExeIfExists(otherNode.FileName), StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

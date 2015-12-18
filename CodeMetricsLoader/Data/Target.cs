using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Target : Node
    {        
        public string FileName { get; }
        public IList<Module> Modules { get; set; }
        public override string Key { get; }

        public override int? Value { get { return (int?) Modules.Average(s => s.Value); } set {} }

        //Classes and structs do not support variance.
        public override IList<Node> Children => (Modules as IEnumerable<Node>).ToList();

        public Target(string name) : base (name)
        {
            FileName = System.IO.Path.GetFileName(Name);
            Key = "Target-" + DropDllOrExeIfExists(FileName);
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
    }
}

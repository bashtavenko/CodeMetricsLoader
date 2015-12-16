using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Module : Node
    {     
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public Metrics Metrics { get; set; }        
        public List<Namespace> Namespaces { get; set; }
        public override string Key { get { return "Module-" + Name; } }
        
        public override int? Value
        {
            get { return Metrics.CodeCoverage; }
            set { Metrics.CodeCoverage = value; }
        }

        public override IList<Node> Children
        {
            get
            {
                //Classes and structs do not support variance.
                return (Namespaces as IEnumerable<Node>).ToList();
            }
        }
        
        public Module ()
        {
            Namespaces = new List<Namespace>();
            Metrics = new Metrics();
        }

        public override void AddChild(Node child)
        {
            if (!(child is Namespace))
            {
                throw new ArgumentException("Must have a namespace node");
            }

            Namespaces.Add((Namespace) child);
        }

        public override bool Equals(object obj)
        {
            // Extension of the modules that come during code coverage merge is unknown because ReportGenerator does not provide it.
            // Most likely it is ".dll", but it can be ".exe" as well.
            var otherNode = obj as Module;
            return otherNode != null && DropDllOrExeIfExists(Name).Equals(DropDllOrExeIfExists(otherNode.Name), StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

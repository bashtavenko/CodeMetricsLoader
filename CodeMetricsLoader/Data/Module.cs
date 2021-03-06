﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Module : Node
    {     
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
        public Metrics Metrics { get; set; }        
        public List<Namespace> Namespaces { get; set; }
        public override string Key { get; }

        public override int? Value
        {
            get { return Metrics.CodeCoverage; }
            set { Metrics.CodeCoverage = value; }
        }

        public override IList<Node> Children => (Namespaces as IEnumerable<Node>).ToList();

        public Module (string name) :base(name)
        {
            Key = "Module-" + DropDllOrExeIfExists(name);
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
    }
}

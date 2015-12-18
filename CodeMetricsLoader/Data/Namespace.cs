using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Namespace : Node
    {     
        public List<Type> Types { get; set; }
        public Metrics Metrics { get; set; }
        public override string Key { get; }
        
        public override int? Value
        {
            get { return Metrics.CodeCoverage; }
            set { Metrics.CodeCoverage = value; }
        }

        //Classes and structs do not support variance.
        public override IList<Node> Children => (Types as IEnumerable<Node>).ToList();

        public Namespace(string name) : base (name)
        {
            Types = new List<Type>();
            Metrics = new Metrics();
            Key = "Namespace-" + Name;
        }

        public override void AddChild(Node child)
        {
            if (!(child is Type))
            {
                throw new ArgumentException("Must have a Type node");
            }

            Types.Add((Type) child);
        }

        public void UpdateMetricsFromTypes()
        {
            double? codeCoverage = Types.Average(s => s.Metrics.CodeCoverage);

            Metrics = new Metrics
            {
                ClassCoupling = (int)Types.Average(s => s.Metrics.ClassCoupling),
                CodeCoverage = codeCoverage.HasValue ? (int) codeCoverage : 0,
                CyclomaticComplexity = Types.Sum(s => s.Metrics.CyclomaticComplexity),
                DepthOfInheritance = (int)Types.Average(s => s.Metrics.DepthOfInheritance),
                LinesOfCode = Types.Sum(s => s.Metrics.LinesOfCode),
                MaintainabilityIndex = (int)Types.Average(s => s.Metrics.MaintainabilityIndex)
            };
        }
    }
}

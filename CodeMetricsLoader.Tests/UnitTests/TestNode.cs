using System;
using System.Collections.Generic;
using System.Linq;
using CodeMetricsLoader.Data;

namespace CodeMetricsLoader.Tests.UnitTests
{
    public class TestNode : Node
    {
        public override IList<Node> Children
        {
            get
            {
                return (TestNodeChildren as IEnumerable<Node>).ToList();
            }
        }
        
        public List<TestNode> TestNodeChildren { get; set; }

        public override string Key { get; }


        public TestNode(string name) : base (name)
        {
            TestNodeChildren = new List<TestNode>();
            Key = "Test-" + Name;
        }

        public override void AddChild(Node child)
        {
            TestNodeChildren.Add(child as TestNode);
        }
    }
}

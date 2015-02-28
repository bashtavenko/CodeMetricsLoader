using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CodeMetricsLoader.Data;

namespace CodeMetricsLoader.CodeCoverage
{
    public class Tree
    {
        private readonly Node _root;

        public Node Root { get { return _root; } }

        public Tree(Node root)
        {
            if (root == null)
            {
                throw new ArgumentException("Must have root", "root");
            }
            _root = root;
        }

        public Node FindNode(string key)
        {
            if (key == null)
            {
                return null;
            }

            return Nodes.FirstOrDefault(n => n.Key == key);
        }

        public IEnumerable<Node> Nodes
        {
            get
            {
                return Traverse(_root);
            }
        }
    
        private IEnumerable<Node> Traverse(Node node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                var nodes = Traverse(child);
                foreach (var n in nodes)
                {
                    yield return n;
                }
            }
        }
    }
}

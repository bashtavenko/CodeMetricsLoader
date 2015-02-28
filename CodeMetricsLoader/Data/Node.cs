using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace CodeMetricsLoader.Data
{
    public class Node
    {
        public virtual string Key { get; set; }
        public virtual int? Value { get; set; }
        public string Name { get; set; }
        public MergeStatus MergeStatus { get; set; }
        public virtual IList<Node> Children { get { return null; }}
        

        public Node()
        {
            MergeStatus = MergeStatus.Unknown;
        }

        public IList<Node> UniqueChildren
        {
            get
            {
                var uniqueKeys = Children
                    .GroupBy(g => g.Key)
                    .Where(w => w.Count() == 1)
                    .ToList();

                return Children.Join(uniqueKeys, c => c.Key, u => u.Key, (c, u) => c).ToList();
            }
        }

        public void UpdateValue(int? value)
        {
            this.Value = this.Value.HasValue ? this.Value + value : value;
            MergeStatus = MergeStatus.Updated;
        }

        public virtual void AddChild(Node child)
        {
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherNode = obj as Node;
            return otherNode != null && otherNode.Key.Equals(this.Key, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

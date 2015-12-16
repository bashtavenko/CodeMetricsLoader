using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodeMetricsLoader.Data
{
    public abstract class Node
    {
        public virtual string Key { get; set; }
        public virtual int? Value { get; set; }
        public string Name { get; set; }
        public MergeStatus MergeStatus { get; set; }
        public virtual IList<Node> Children { get { return null; }}
        
        // There are no code coverage for members.
        public virtual bool CanBeMerged { get; set; } = true;
        
        public Node()
        {
            MergeStatus = MergeStatus.Unknown;
        }

        /// <summary>
        /// Filter out dups, leave only unique members.
        /// </summary>
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

        // We can't use Path.GetFileNameWithoutExtension because we only want to drop .dll or .exe
        // and Path.GetFileNameWithoutExtension doesn't work in cases like "A.Data.Maps" (it will think that extension is .Maps)
        // We also can't make it a property since Target has FileNameWithoutExtension property.
        public string DropDllOrExeIfExists(string nameToCheck)
        {
            if (nameToCheck == null)
            {
                throw new ArgumentNullException(nameof(nameToCheck));
            }
            var extensionIndex = new Regex(".dll|.exe").Match(nameToCheck).Index;
            if (extensionIndex > 0)
            {
                return nameToCheck.Substring(0, extensionIndex);
            }
            else
            {
                return nameToCheck;
            }
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherNode = obj as Node;
            return otherNode != null && otherNode.Key.Equals(this.Key, StringComparison.InvariantCulture); // Do NOT ignore case
        }
    }
}

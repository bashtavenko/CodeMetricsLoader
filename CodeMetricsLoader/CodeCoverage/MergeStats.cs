using System;
using System.Collections.Generic;
using CodeMetricsLoader.Data;

namespace CodeMetricsLoader.CodeCoverage
{
    public class MergeStats
    {
        public int UpdatedCount { get; private set; }
        public int TotalNodesCount { get; private set; }

        public IList<Node> SkippedNodes { get; private set; }
        public IList<Node> NewInThisTreeNodes { get; private set; }
        public IList<Node> MissingInOtherTreeNodes { get; private set; }
    
        public MergeStats()
        {
            SkippedNodes = new List<Node>();
            NewInThisTreeNodes = new List<Node>();
            MissingInOtherTreeNodes = new List<Node>();
        }

        public decimal MergeRatio
        {
            get
            {
                if (TotalNodesCount == 0)
                {
                    return 0;
                }
                else
                {
                    return (decimal) UpdatedCount/(TotalNodesCount - SkippedNodes.Count);
                }
            }
        }

        public static MergeStats Calculate(Tree tree)
        {
            if (tree == null)
            {
                throw new ArgumentException("Must have tree");
            }

            var stats = new MergeStats();

            foreach (var node in tree.Nodes)
            {
                stats.TotalNodesCount++;
                if (!node.CanBeMerged)
                {
                    stats.SkippedNodes.Add(node);
                    continue;
                }
                switch (node.MergeStatus)
                {
                    case MergeStatus.Unknown:
                        stats.SkippedNodes.Add(node);
                        break;
                    case MergeStatus.Updated:
                        stats.UpdatedCount++;
                        break;
                    case MergeStatus.NewInThisTree:
                        stats.NewInThisTreeNodes.Add(node);
                        break;
                    case MergeStatus.MissingInOtherTree:
                        stats.MissingInOtherTreeNodes.Add(node);
                        break;
                }
            }
            return stats;
        }
    }
}

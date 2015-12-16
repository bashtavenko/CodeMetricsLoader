using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Services;
using CodeMetricsLoader.Data;

namespace CodeMetricsLoader.CodeCoverage
{
    public class Merger
    {
        /// <summary>
        /// Merge metrics (maintainablity index, cyclomatic complexity, etc) with code coverage
        /// </summary>
        /// <param name="metrics">Typically one-item collection</param>
        /// <param name="codeCoverage">List of all targets with code coverage</param>
        /// <param name="logger">Logger to report progress</param>
        /// <returns>List of targets with combined metrics and code coverage</returns>
        public IList<Target> Merge(IList<Target> metrics, IList<Target> codeCoverage, ILogger logger)
        {
            if (metrics == null || codeCoverage == null)
            {
                throw new ArgumentException("Must have metrics and code coverage");
            }

            var result = new List<Target>();
            foreach (var metricsTarget in metrics)
            {
                var codeCoverageTarget = codeCoverage.SingleOrDefault(s => s.Equals(metricsTarget)); // There should always be 0..1 code coverage targets.
                if (codeCoverageTarget != null)
                {
                    var treeA = new Tree(metricsTarget);
                    var treeB = new Tree(codeCoverageTarget);
                    
                    // Updates in mergedTree == treeA == metricsTarget
                    var mergedTree = Merge(treeA, treeB, MergeMode.OneWay); 
                    
                    var stats = MergeStats.Calculate(mergedTree);
                    logger.Log(string.Format(@"Code coverage merge stats - Total nodes:{0:N0} Merged:{1:N0} Merge Ratio:{2:P0} Missing Code Coverage:{3:N0} Ignored:{4:N0}",
                        stats.TotalNodesCount, stats.UpdatedCount, stats.MergeRatio, stats.MissingInOtherTreeNodes.Count, stats.SkippedNodes.Count));
                }
                result.Add(metricsTarget);
            }
            return result;
        }

        public Tree Merge(Tree treeA, Tree treeB, MergeMode mergeMode)
        {
            if (treeA == null || treeB == null)
            {
                throw new ArgumentException("Must have trees");
            }

            if (!treeA.Root.Equals(treeB.Root))
            {
                throw new ArgumentException("Trees must have the same root key");
            }

            MergeNode(treeA.Root, treeB.Root, mergeMode);
            return treeA;
        }

        private void MergeNode(Node nodeA, Node nodeB, MergeMode mergeMode)
        {
            if (nodeA.MergeStatus != MergeStatus.Updated)
            {
                nodeA.UpdateValue(nodeB.Value);
            }

            // NodeA => NodeB
            foreach (var childNodeA in nodeA.UniqueChildren)
            {
                var childNodeB = nodeB.UniqueChildren.SingleOrDefault(s => s.Equals(childNodeA));
                if (childNodeA.Equals(childNodeB))
                {
                    childNodeA.UpdateValue(childNodeB.Value);
                }
                else
                {
                    childNodeA.MergeStatus = MergeStatus.MissingInOtherTree;
                }
            }

            // NodeB => NodeA
            if (mergeMode == MergeMode.BothWays)
            {
                foreach (var childNodeB in nodeB.UniqueChildren)
                {
                    if (nodeA.UniqueChildren.SingleOrDefault(s => s.Equals(childNodeB)) == null)
                    {
                        nodeA.AddChild(childNodeB);
                        childNodeB.MergeStatus = MergeStatus.NewInThisTree;
                    }
                }
            }

            // Keep on going down with the updated nodes
            foreach (var newNodeA in nodeA.UniqueChildren.Where(c => c.MergeStatus == MergeStatus.Updated && c.UniqueChildren.Any()))
            {
                var newNodeB = nodeB.UniqueChildren.Single(c => c.Equals(newNodeA));
                MergeNode(newNodeA, newNodeB, mergeMode);
            }
        }
    }
}

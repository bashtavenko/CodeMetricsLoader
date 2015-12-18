using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeMetricsLoader.CodeCoverage;
using CodeMetricsLoader.Data;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class MergerTests
    {
        private Merger _merger;

        [TestFixtureSetUp]
        public void Setup()
        {
            _merger = new Merger();
        }

        [Test]
        public void Merger_TreeNodes()
        {   
            // Arrange
            var nodeA = new TestNode ("A")
                        {
                            Value = 2,
                            TestNodeChildren = new List<TestNode>
                                               {
                                                 new TestNode ("B") { Value = 3},   
                                                 new TestNode ("C") { Value = 1}   
                                               }
                        };

            var nodeB = new TestNode ("A")
            {
                Value = 3,
                TestNodeChildren = new List<TestNode>
                                               {
                                                 new TestNode ("B") { Value = 1},   
                                                 new TestNode ("D") { Value = 2}   
                                               }
            };

            var treeA = new Tree(nodeA);
            var treeB = new Tree(nodeB);

            // Act
            Tree result = _merger.Merge(treeA, treeB, MergeMode.BothWays);

            // Assert
            Assert.IsNotNull(result);
            var list = result.Nodes.ToList();
            Assert.That(list.Count, Is.EqualTo(4));

            Node node = list[0];
            Assert.That(node.Name, Is.EqualTo("A"));
            Assert.That(node.Value, Is.EqualTo(5));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.Updated));

            node = list[1];
            Assert.That(node.Name, Is.EqualTo("B"));
            Assert.That(node.Value, Is.EqualTo(4));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.Updated));

            node = list[2];
            Assert.That(node.Name, Is.EqualTo("C"));
            Assert.That(node.Value, Is.EqualTo(1));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.MissingInOtherTree));

            node = list[3];
            Assert.That(node.Name, Is.EqualTo("D"));
            Assert.That(node.Value, Is.EqualTo(2));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.NewInThisTree));
        }

        [Test]
        public void Merger_FourNodes()
        {
            // Arrange
            var nodeA = new TestNode ("A")
            {
                Value = 2,
                TestNodeChildren = new List<TestNode>
                {
                    new TestNode ("B")
                    {
                        Value = 3,
                        TestNodeChildren = new List<TestNode>
                        {
                            new TestNode ("C")
                            {
                                Value = 4,
                                TestNodeChildren = new List<TestNode>
                                {
                                    new TestNode ("D1") { Value = 2},
                                    new TestNode ("D2") { Value = 3},
                                }
                            }               
                        }
                    },   
                }
            };

            var nodeB = new TestNode ("A")
            {
                Value = 1,
                TestNodeChildren = new List<TestNode>
                {
                    new TestNode ("B")
                    {
                        Value = 2,
                        TestNodeChildren = new List<TestNode>
                        {
                            new TestNode ("C")
                            {
                                Value = 1,
                                TestNodeChildren = new List<TestNode>
                                {
                                    new TestNode ("D1") { Value = 10},
                                    new TestNode ("D2") { Value = 20},
                                    new TestNode ("D3") { Value = 30},
                                }                    
                            }               
                        }
                    },   
                }
            };

            var treeA = new Tree(nodeA);
            var treeB = new Tree(nodeB);

            // Act
            Tree result = _merger.Merge(treeA, treeB, MergeMode.BothWays);

            // Assert
            Assert.IsNotNull(result);
            var list = result.Nodes.ToList();
            Assert.That(list.Count, Is.EqualTo(6));

            Node node = list[0];
            Assert.That(node.Name, Is.EqualTo("A"));
            Assert.That(node.Value, Is.EqualTo(3));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.Updated));

            node = list[1];
            Assert.That(node.Name, Is.EqualTo("B"));
            Assert.That(node.Value, Is.EqualTo(5));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.Updated));

            node = list[2];
            Assert.That(node.Name, Is.EqualTo("C"));
            Assert.That(node.Value, Is.EqualTo(5));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.Updated));

            node = list[3];
            Assert.That(node.Name, Is.EqualTo("D1"));
            Assert.That(node.Value, Is.EqualTo(12));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.Updated));

            node = list[4];
            Assert.That(node.Name, Is.EqualTo("D2"));
            Assert.That(node.Value, Is.EqualTo(23));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.Updated));

            node = list[5];
            Assert.That(node.Name, Is.EqualTo("D3"));
            Assert.That(node.Value, Is.EqualTo(30));
            Assert.That(node.MergeStatus, Is.EqualTo(MergeStatus.NewInThisTree));
        }
    }
}

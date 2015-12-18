using System;
using System.Collections.Generic;
using System.Linq;
using CodeMetricsLoader.CodeCoverage;
using CodeMetricsLoader.Data;
using NUnit.Framework;
using Type = CodeMetricsLoader.Data.Type;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class TreeTests
    {

        [Test]
        public void Tree_Nodes_Count()
        {
            // Arrange
            var node = new TestNode ("A")
            {
                Value = 2,
                TestNodeChildren = new List<TestNode>
                {
                    new TestNode ("B") {Value = 3},   
                    new TestNode ("C") {Value = 1},
                    new TestNode ("D") {Value = 1},
                    new TestNode ("E") {Value = 1}   
                }
            };
            var tree = new Tree(node);

            // Act
            var nodes = tree.Nodes;

            // Assert
            Assert.That(nodes.Count(), Is.EqualTo(5));
        }

        [Test]
        public void Tree_Find()
        {
            // Arrange
            var target = new Target ("T1")
                         {
                             Modules = new List<Module>
                                       {
                                           new Module ("M1")
                                           {
                                               Metrics = new Metrics { CodeCoverage = 10},
                                               Namespaces = new List<Namespace>
                                                            {
                                                                new Namespace ("NS1")
                                                                {
                                                                    Metrics = new Metrics { CodeCoverage = 10},
                                                                    Types = new List<Type>
                                                                            {
                                                                                new Type ("TY1")
                                                                                {
                                                                                    Metrics = new Metrics { CodeCoverage = 10},
                                                                                    Members = new List<Member>
                                                                                              {
                                                                                                  new Member ("ME1")
                                                                                                  {
                                                                                                      Metrics = new Metrics { CodeCoverage = 10},
                                                                                                  },
                                                                                                  new Member ("ME2")
                                                                                                  {
                                                                                                      Metrics = new Metrics { CodeCoverage = 10},
                                                                                                  }  
                                                                                              }
                                                                                }
                                                                            }
                                                                }
                                                            }
                                           }
                                       }
                         };
            Node node = target;
            var tree = new Tree(node);
            
            // Act - Assert
            var nodes = tree.Nodes.ToList();
            Assert.That(nodes.Count, Is.EqualTo(6));
            Assert.IsNotNull(tree.FindNode("Target-T1"));
            Assert.IsNotNull(tree.FindNode("Module-M1"));
            Assert.IsNotNull(tree.FindNode("Namespace-NS1"));
            Assert.IsNotNull(tree.FindNode("Type-TY1"));
            Assert.IsNotNull(tree.FindNode("Member-ME1"));
            Assert.IsNotNull(tree.FindNode("Member-ME2"));
            Assert.IsNull(tree.FindNode("Bogus"));
        }

        [Test]
        public void Tree_With_Dups()
        {
            // Arrange
            var node = new TestNode ("A")
            {
                Value = 2,
                TestNodeChildren = new List<TestNode>
                {
                    new TestNode ("B") { Value = 3},   
                    new TestNode ("B") { Value = 1},
                    new TestNode ("C") { Value = 1},
                    new TestNode ("D") { Value = 1},   
                    new TestNode ("D") { Value = 3}   
                }                 
            };
            var tree = new Tree(node);

            // Act
            var nodes = tree.Nodes;

            // Assert
            Assert.That(nodes.Count(), Is.EqualTo(6));

        }
    }
}
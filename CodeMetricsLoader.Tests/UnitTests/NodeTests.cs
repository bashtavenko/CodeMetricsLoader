using System.Collections.Generic;
using CodeMetricsLoader.CodeCoverage;
using CodeMetricsLoader.Data;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class NodeTests
    {
        [Test]
        public void INode_Member()
        {
            var member = new Member {Metrics = new Metrics {CodeCoverage = 25}, Name = "Abc"};
            Assert.AreEqual (25, member.Value);
            
            // Since classes and structs do not support variance, this won't compile:
            //List<INode> nodeList = members.ToList();
        }

        [Test]
        public void INode_UniqueChildren()
        {
            // Arrange
            var type = new Type
            {
                Name = "TY1",
                Members = new List<Member>
                {
                    new Member {Name = "B"},
                    new Member {Name = "B"},
                    new Member {Name = "C"},
                    new Member {Name = "D"},
                    new Member {Name = "D"},
                    new Member {Name = "F"},
                }
            };

            // Act
            var nodes = type.UniqueChildren;

            // Assert
            Assert.That(nodes.Count, Is.EqualTo(2));
            Assert.That(nodes[0].Name, Is.EqualTo("C"));
            Assert.That(nodes[1].Name, Is.EqualTo("F"));
        }

        [Test]
        public void INode_Value()
        {
            // Arrange
            var type = new Type
            {
                Name = "TY1",
                Value = 10
            };

            // Act
            type.UpdateValue(20);

            // Assert
            Assert.That(type.Value, Is.EqualTo(30));
        }

        [Test]
        public void INode_Value_2()
        {
            // Arrange
            var type = new Type
            {
                Name = "TY1",
            };

            // Act
            type.UpdateValue(20);

            // Assert
            Assert.That(type.Value, Is.EqualTo(20));
        }
    }
}

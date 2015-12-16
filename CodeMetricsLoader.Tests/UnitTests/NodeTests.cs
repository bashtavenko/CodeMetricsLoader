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
        public void INode_UniqueChildren_WithCase()
        {
            // Arrange
            var type = new Type
            {
                Name = "TY1",
                Members = new List<Member>
                {
                    new Member {Name = "B"},
                    new Member {Name = "B"},
                    new Member {Name = "b"},
                    new Member {Name = "C"},
                    new Member {Name = "D"},
                    new Member {Name = "D"},
                    new Member {Name = "F"},
                }
            };

            // Act
            var nodes = type.UniqueChildren;

            // Assert
            Assert.That(nodes.Count, Is.EqualTo(3));
            Assert.That(nodes[0].Name, Is.EqualTo("b"));
            Assert.That(nodes[1].Name, Is.EqualTo("C"));
            Assert.That(nodes[2].Name, Is.EqualTo("F"));
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

        [TestCase("A", "A", true)]
        [TestCase("A.dll", "A", true)]
        [TestCase("A.exe", "A", true)]
        [TestCase("A", "B", false)]
        [TestCase("A", "A.dll", true)]
        [TestCase("A", "A.exe", true)]
        [TestCase("A.Data.Maps.dll", "A.Data.Maps", true)]
        [TestCase("A.Data.Maps", "A.Data.Maps.dll", true)]
        [TestCase("A.Data.Maps.exe", "A.Data.Maps", true)]
        [TestCase("A.Data.Maps", "A.Data.Maps.exe", true)]
        [TestCase("a.data.maps", "A.Data.Maps.exe", false)]
        public void INode_Module(string firstModule, string secondModule, bool expected)
        {
            Assert.That(new Module {Name = firstModule}.Equals(new Module {Name = secondModule}), Is.EqualTo(expected));
        }
    }
}

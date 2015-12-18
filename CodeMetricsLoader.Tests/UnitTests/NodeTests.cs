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
        public void Node_Member()
        {
            var member = new Member ("Abc") {Metrics = new Metrics {CodeCoverage = 25}};
            Assert.AreEqual (25, member.Value);
            
            // Since classes and structs do not support variance, this won't compile:
            //List<Node> nodeList = members.ToList();
        }

        [Test]
        public void Node_UniqueChildren()
        {
            // Arrange
            var type = new Type ("TY1")
            {
                Members = new List<Member>
                {
                    new Member ("B"),
                    new Member ("B"),
                    new Member ("C"),
                    new Member ("D"),
                    new Member ("D"),
                    new Member ("F"),
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
        public void Node_UniqueChildren_WithCase()
        {
            // Arrange
            var type = new Type ("TY1")
            {
                Members = new List<Member>
                {
                    new Member ("B"),
                    new Member ("B"),
                    new Member ("b"),
                    new Member ("C"),
                    new Member ("D"),
                    new Member ("D"),
                    new Member ("F"),
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
        public void Node_Value()
        {
            // Arrange
            var type = new Type ("TY1")
            {
                Value = 10
            };

            // Act
            type.UpdateValue(20);

            // Assert
            Assert.That(type.Value, Is.EqualTo(30));
        }

        [Test]
        public void Node_Value_2()
        {
            // Arrange
            var type = new Type("TY1");
            
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
        public void Node_Module(string firstModule, string secondModule, bool expected)
        {
            Assert.That(new Module (firstModule).Equals(new Module (secondModule)), Is.EqualTo(expected));
        }

        [TestCase("c:/src/A.Data.Maps.dll", "A.Data.Maps", true)]
        [TestCase("A.Data.Maps", "c:/src/A.Data.Maps.dll", true)]
        [TestCase(@"C:\My\Playground\CodeMetricsLoader\CodeMetricsLoader\bin\Debug\CodeMetricsLoader.exe", "CodeMetricsLoader", true)]
        [TestCase("CodeMetricsLoader", @"C:\My\Playground\CodeMetricsLoader\CodeMetricsLoader\bin\Debug\CodeMetricsLoader.exe", true)]
        public void Node_Target(string firstTarget, string secondTarget, bool expected)
        {
            Assert.That(new Target(firstTarget).Equals(new Target(secondTarget)), Is.EqualTo(expected));
        }
    }
}

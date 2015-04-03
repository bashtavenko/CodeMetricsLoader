using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml.Linq;
using CodeMetricsLoader.CodeCoverage;
using CodeMetricsLoader.Data;
using CodeMetricsLoader.Tests.IntegrationTests;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class OpenCoverParserTests
    {
        private OpenCoverParser _parser;

        [TestFixtureSetUp]
        public void Setup()
        {
            _parser = new OpenCoverParser();
        }
        
        [Test]
        public void Parse()
        {
            // Arrange
            var elements = LoadXml("codecoverage.xml");      

            // Act
            List<Target> targets = _parser.Parse(elements);

            // Assert
            Assert.AreEqual(1, targets.Count);
            var target = targets.First();
            Assert.AreEqual(@"C:\Jenkins\workspace\Test.dll", target.Name);
            Assert.AreEqual("Test.dll", target.FileName);

            Assert.AreEqual(1, targets.First().Modules.Count);
            var module = targets.First().Modules.First();
            Assert.AreEqual("Test.dll", module.Name);
            Assert.AreEqual(2, module.Namespaces.Count);

            var ns = module.Namespaces[0];
            Assert.AreEqual("Test", ns.Name);
            Assert.IsNotNull(ns.Metrics);
            Assert.AreEqual(40, ns.Metrics.CodeCoverage);

            Assert.AreEqual(1, ns.Types.Count);
            var type = ns.Types.First();
            Assert.AreEqual("AutoMapperConfig", type.Name);

            Assert.AreEqual(2, type.Members.Count);
            var method = type.Members[0];
            Assert.AreEqual("CreateMaps() : void", method.Name);
            Assert.IsNotNull(method.Metrics);
            Assert.AreEqual(50, method.Metrics.CodeCoverage);
            method = type.Members[1];
            Assert.AreEqual("ShortenText(string) : string", method.Name);
            Assert.AreEqual(60, method.Metrics.CodeCoverage);

            ns = module.Namespaces[1];
            Assert.AreEqual("Test.Data", ns.Name);

            Assert.AreEqual(1, ns.Types.Count);
            type = ns.Types.First();
            Assert.AreEqual("Encryptor", type.Name);
            Assert.AreEqual(1, type.Members.Count);
            method = type.Members[0];
            Assert.AreEqual("CreateMaps() : int", method.Name);
            Assert.AreEqual(30, method.Metrics.CodeCoverage);
        }
        
        public static XElement LoadXml(string fileName)
        {
            return LoadXmlInternal(fileName);
        }

        private static XElement LoadXmlInternal(string fileName)
        {
            using (var sr = new StreamReader(Path.Combine("..\\..\\TestFiles\\", fileName)))
            {
                string xml = sr.ReadToEnd();
                return XElement.Parse(xml);
            }
        }
    }
}
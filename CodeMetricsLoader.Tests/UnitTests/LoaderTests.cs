using System;
using System.Xml.Linq;
using System.IO;
using CodeMetricsLoader.Data;
using NUnit.Framework;
using System.Collections.Generic;
using CodeMetricsLoader.Tests.IntegrationTests;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class LoaderTests
    {
        private XElement _elements;
        private Loader _loader;
        private LoaderContext _context;

        [TestFixtureSetUp]
        public void Setup()
        {
            _elements = LoadXml();            
            _context = ContextTests.CreateTestContext();
            _loader = new Loader(_context, new TestLogger());
        }

        [Test]
        public void Loader_MapXml_Regular()
        {
            List<Target> targets = _loader.MapXmlToEntities(_elements);
            Assert.AreEqual(1, targets.Count);
            var target = targets[0];
            Assert.IsNotEmpty(target.Name);

            Assert.AreEqual(1, target.Modules.Count);
            var module = target.Modules[0];
            Assert.IsNotEmpty(module.Name);
            Assert.IsNotEmpty(module.AssemblyVersion);
            Assert.IsNotEmpty(module.FileVersion);
                       

            Assert.AreEqual(1, module.Namespaces.Count);
            var ns = module.Namespaces[0];
            

            Assert.AreEqual(16, ns.Types.Count);
            var type = ns.Types[1];

            Assert.AreEqual(6, type.Members.Count);
            var member = type.Members[0];
            Assert.IsNotNull(member.Metrics);

            var metrics = member.Metrics;
            Assert.IsTrue(metrics.ClassCoupling > 0 || metrics.CyclomaticComplexity > 0 ||
                metrics.DepthOfInheritance > 0 || metrics.LinesOfCode > 0 ||
                metrics.MaintainabilityIndex > 0);
        }        

        public static XElement LoadXml()
        {
            return LoadXml("CodeMetricsResult.xml");            
        }

        private static XElement LoadXml(string fileName)
        {
            using (StreamReader sr = new StreamReader(Path.Combine("..\\..\\TestFiles\\", fileName)))
            {
                string xml = sr.ReadToEnd();
                return XElement.Parse(xml);
            }
        }
    }
}

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
            List<Target> targets = _loader.MapXmlToEntities(_elements, "master");
            Assert.AreEqual(1, targets.Count);
            var target = targets[0];
            Assert.IsNotEmpty(target.Name);

            Assert.AreEqual(1, target.Modules.Count);
            var module = target.Modules[0];
            Assert.IsNotEmpty(module.Name);
            Assert.IsNotEmpty(module.AssemblyVersion);
            Assert.IsNotEmpty(module.FileVersion);

            Assert.IsNotNull(module.Metrics);
            var moduleMetrics = module.Metrics;
            Assert.IsTrue(moduleMetrics.ClassCoupling > 0 || moduleMetrics.CyclomaticComplexity > 0 ||
                moduleMetrics.DepthOfInheritance > 0 || moduleMetrics.LinesOfCode > 0 ||
                moduleMetrics.MaintainabilityIndex > 0);

            Assert.AreEqual(1, module.Namespaces.Count);
            var ns = module.Namespaces[0];
            Assert.IsNotNull(ns.Metrics);

            Assert.AreEqual(16, ns.Types.Count);
            var type = ns.Types[1];

            Assert.AreEqual(6, type.Members.Count);
            var member = type.Members[0];
            Assert.IsNotNull(member.Metrics);            
        }

        [Test]        
        public void Loader_MapXml_Commas()        
        {
            XElement el = LoadXml("NumbersWithCommas.xml");
            List<Target> targets = _loader.MapXmlToEntities(el, string.Empty);

            Assert.AreEqual(1, targets.Count);
            var target = targets[0];
            Assert.IsNotEmpty(target.Name);

            Assert.AreEqual(1, target.Modules.Count);
            var module = target.Modules[0];
            Assert.IsNotEmpty(module.Name);
            Assert.IsNotEmpty(module.AssemblyVersion);
            Assert.IsNotEmpty(module.FileVersion);

            Assert.IsNotNull(module.Metrics);
            var moduleMetrics = module.Metrics;
            Assert.AreEqual(14363, moduleMetrics.LinesOfCode);
            Assert.AreEqual(87, moduleMetrics.MaintainabilityIndex);
            Assert.AreEqual(7559, moduleMetrics.CyclomaticComplexity);
            Assert.AreEqual(265, moduleMetrics.ClassCoupling);
            Assert.AreEqual(3, moduleMetrics.DepthOfInheritance);            
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

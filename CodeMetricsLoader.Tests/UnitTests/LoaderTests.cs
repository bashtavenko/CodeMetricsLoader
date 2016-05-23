using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

using CodeMetricsLoader.Data;
using CodeMetricsLoader.Tests.IntegrationTests;
using Moq;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class LoaderTests
    {
        private XElement _elements;
        private Loader _loader;
        private LoaderContext _context;
        private Mock<IMetricsRepository> _repoMock;

        [TestFixtureSetUp]
        public void Setup()
        {
            _elements = LoadXml();            
            _context = ContextTests.CreateTestContext();
            _repoMock = new Mock<IMetricsRepository>();
            _loader = new Loader(_repoMock.Object, new TestLogger());
        }

        [Test]
        public void Loader_MapXml_Regular()
        {
            List<Target> targets = _loader.MapMetricsXmlToEntities(_elements);
            Assert.AreEqual(1, targets.Count);
            var target = targets[0];
            Assert.IsNotEmpty(target.Name);
            Assert.That(target.Key, Is.EqualTo("Target-WebServices.Inbound"));

            Assert.AreEqual(1, target.Modules.Count);
            var module = target.Modules[0];
            Assert.IsNotEmpty(module.Name);
            Assert.IsNotEmpty(module.AssemblyVersion);
            Assert.IsNotEmpty(module.FileVersion);
            Assert.That(module.Key, Is.EqualTo("Module-WebServices.Inbound"));

            Assert.AreEqual(1, module.Namespaces.Count);
            var ns = module.Namespaces[0];
            Assert.That(ns.Key, Is.EqualTo("Namespace-WebServices.Inbound"));
            Assert.AreEqual(16, ns.Types.Count);
            var type = ns.Types[1];
            Assert.That(type.Key, Is.EqualTo("Type-IQpayService"));

            Assert.AreEqual(6, type.Members.Count);
            var member = type.Members[0];
            Assert.IsNotNull(member.Metrics);
            Assert.That(member.Key, Is.EqualTo("Member-AccountLookup(AccountLookupRequest) : AccountLookupResponse"));

            var metrics = member.Metrics;
            Assert.IsTrue(metrics.ClassCoupling > 0 || metrics.CyclomaticComplexity > 0 ||
                metrics.DepthOfInheritance > 0 || metrics.LinesOfCode > 0 ||
                metrics.MaintainabilityIndex > 0);

            var testType = targets
                .SelectMany(t => t.Modules)
                .SelectMany(m => m.Namespaces)
                .SelectMany(m => m.Types)
                .SingleOrDefault(t => t.Name == "QpayService");
            
            Assert.IsNotNull(testType);

            var testMember = testType.Members.SingleOrDefault(f => f.Name == "AccountLookup(AccountLookupRequest) : AccountLookupResponse");
            Assert.IsNotNull(testMember);
            Assert.IsNotNull(testMember.File);
            Assert.IsNotNull(testMember.Line);
        }
        
        public static XElement LoadXmlFromAbsolutePath(string absolutePathFileName)
        {
            using (StreamReader sr = new StreamReader(absolutePathFileName))
            {
                string xml = sr.ReadToEnd();
                return XElement.Parse(xml);
            }
        }


        public static XElement LoadXml(string fileName)
        {
            return LoadXmlInternal(fileName);
        }
        

        public static XElement LoadXml()
        {
            return LoadXmlInternal("CodeMetricsResult.xml");            
        }

        private static XElement LoadXmlInternal(string fileName)
        {
            using (StreamReader sr = new StreamReader(Path.Combine("..\\..\\TestFiles\\", fileName)))
            {
                string xml = sr.ReadToEnd();
                return XElement.Parse(xml);
            }
        }
    }
}

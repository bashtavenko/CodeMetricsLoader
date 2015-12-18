using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CodeMetricsLoader.CodeCoverage;
using CodeMetricsLoader.Data;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class CodeCoverageConverterTests
    {
        private CodeCoverageConverter _converter;

        [TestFixtureSetUp]
        public void Setup()
        {
            _converter = new CodeCoverageConverter();
        }

        [Test]
        public void CodeCoverageConverter_Convert()
        {
            var input = LoadXmlInternal("CodeCoverageSummary.xml");
            List<Target> result = _converter.Convert(input);
            Assert.That(result.Count, Is.EqualTo(1));
            var module = result.First().Modules.First();
            Assert.That(module.Name, Is.EqualTo("CodeMetricsLoader"));
            Assert.That(module.Metrics.CodeCoverage, Is.EqualTo(88));
            Assert.That(module.Namespaces.Count, Is.EqualTo(4));
        }

        [Test]
        public void CodeCoverageConverter_Convert_InvalidFile()
        {
            var input = LoadXmlInternal("metrics.xml");
            Assert.Throws<LoaderException>(() => _converter.Convert(input));
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
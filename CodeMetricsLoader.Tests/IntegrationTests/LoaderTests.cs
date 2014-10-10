using System.Linq;
using System.Xml.Linq;

using CodeMetricsLoader.Data;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class LoaderTests
    {
        [Test]
        public void Loader_Load_CanSaveXml()
        {
            LoaderContext context = ContextTests.CreateTestContext();            
            Loader loader = new Loader(context, new TestLogger());

            XElement metrics = UnitTests.LoaderTests.LoadXml();
            loader.Load(metrics, "master", false);

            LoaderContext testContext = ContextTests.CreateTestContext();

            // this can fail if running along with other tests
            Assert.AreEqual(130, testContext.Metrics.Count());
            Assert.AreEqual(1, testContext.Dates.Count());
            Assert.AreEqual(1, testContext.Targets.Count());
            Assert.AreEqual(1, testContext.Namespaces.Count());
            Assert.AreEqual(16, testContext.Types.Count());
            Assert.AreEqual(112, testContext.Members.Count());

            var metricsFromDb = testContext.Metrics.Where(m => m.Namespace.Name == "WebServices.Inbound").First();
            Assert.IsNotNull(metricsFromDb);
            Assert.AreEqual(94, metricsFromDb.MaintainabilityIndex);
            Assert.AreEqual(112, metricsFromDb.CyclomaticComplexity);
            Assert.AreEqual(21, metricsFromDb.ClassCoupling);
            Assert.AreEqual(1, metricsFromDb.DepthOfInheritance);
            Assert.AreEqual(112, metricsFromDb.LinesOfCode);            
        }

        [Test]
        public void Loader_Load_CanSaveTheSameXmlTwice()
        {
            LoaderContext context = ContextTests.CreateTestContext();

            var loader = new Loader(context, new TestLogger());

            XElement metrics = UnitTests.LoaderTests.LoadXml();
            loader.Load(metrics, "master", false);
            loader.Load(metrics, "master", false);
            
            // ..no easy way to assert since database is not being drop when run multiple tests in the same class
        }
    }
}

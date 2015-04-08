using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

using CodeMetricsLoader.Data;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class LoaderTests
    {
        [Test]
        public void Loader_Load_CanSaveXml()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {
                Loader loader = new Loader(context, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml();
                loader.Load(metrics, null, false);
            }

            using (LoaderContext testContext = ContextTests.CreateTestContext())
            {             
                Assert.AreEqual(130, testContext.Metrics.Count());
                Assert.AreEqual(1, testContext.Dates.Count());
                Assert.AreEqual(1, testContext.Namespaces.Count());
                Assert.AreEqual(16, testContext.Types.Count());
                Assert.AreEqual(62, testContext.Members.Count());

                var metricsFromDb = testContext.Metrics.FirstOrDefault(m => m.Namespace.Name == "WebServices.Inbound");
                Assert.IsNotNull(metricsFromDb);
                Assert.AreEqual(94, metricsFromDb.MaintainabilityIndex);
                Assert.AreEqual(112, metricsFromDb.CyclomaticComplexity);
                Assert.AreEqual(21, metricsFromDb.ClassCoupling);
                Assert.AreEqual(1, metricsFromDb.DepthOfInheritance);
                Assert.AreEqual(112, metricsFromDb.LinesOfCode);
                
                var type = testContext.Types.SingleOrDefault(t => t.Name == "QpayService");
                Assert.IsNotNull(type);
                Assert.That(type.Members.Count, Is.EqualTo(7));

                var member = type.Members.SingleOrDefault(f => f.Name == "AccountLookup(AccountLookupRequest) : AccountLookupResponse");
                Assert.IsNotNull(member);
                Assert.That(member.Metrics.Count, Is.EqualTo(1));
                Assert.IsNotNull(member.File);

                type = testContext.Types.SingleOrDefault(t => t.Name == "IQpayService");
                Assert.IsNotNull(type);
                Assert.That(type.Members.Count, Is.EqualTo(6));

                member = type.Members.SingleOrDefault(f => f.Name == "AccountLookup(AccountLookupRequest) : AccountLookupResponse");
                Assert.IsNotNull(member);
                Assert.That(member.Metrics.Count, Is.EqualTo(1));
                Assert.IsNull(member.File);
            }
        }

        //[Test]
        public void Loader_Load_CanSaveXmlWithDups()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {
                Loader loader = new Loader(context, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml("DupCheck.xml");
                loader.Load(metrics, null, true);
            }

            using (LoaderContext context = ContextTests.CreateTestContext())
            {
                Loader loader = new Loader(context, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml("DupCheck.xml");
                loader.Load(metrics, null, true);
            }

            using (LoaderContext testContext = ContextTests.CreateTestContext())
            {
                Assert.AreEqual(14, testContext.Metrics.Count());
                Assert.AreEqual(2, testContext.Dates.Count());
                Assert.AreEqual(2, testContext.Namespaces.Count());
                Assert.AreEqual(1, testContext.Types.Count());
                Assert.AreEqual(1, testContext.Members.Count());
            }
        }

        [Test]
        public void Loader_Load_CanSaveTheSameXmlTwice()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {

                var loader = new Loader(context, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml();
                loader.Load(metrics, null, false);
                loader.Load(metrics, null, false);
                
                // TODO: Assert
            }
        }

        [Test]
        public void Loader_LoadMetricsAndCodeCoverage()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {

                var loader = new Loader(context, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml("metrics-A.xml");
                XElement codeCoverage = UnitTests.LoaderTests.LoadXml("codecoverage-A.xml");
                loader.Load(metrics, codeCoverage, false);

                // TODO: Assert
            }
        }
    }
}

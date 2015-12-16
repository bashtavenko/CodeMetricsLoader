using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using NUnit.Framework;

using CodeMetricsLoader.Data;
using Dapper;

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

                XElement metrics = UnitTests.LoaderTests.LoadXml("CodeMetricsLoader.metrics.xml");
                XElement codeCoverage = UnitTests.LoaderTests.LoadXml("CodeMetricsLoader.CodeCoverage.xml");
                loader.Load(metrics, codeCoverage, false);

                var db = context.Database.Connection;
                var modules = db.Query("select * from DimModule").ToList();
                Assert.That(modules.Count(), Is.EqualTo(1));
                var moduleId = modules.First().ModuleId;

                var namespaces = db.Query("select * from DimNamespace").ToList();
                Assert.That(namespaces.Count, Is.EqualTo(4));

                var types = db.Query("select * from DimType").ToList();
                Assert.That(types.Count, Is.EqualTo(66));

                var members = db.Query("select * from DimMember").ToList();
                Assert.That(members.Count, Is.EqualTo(419));

                var moduleMetrics = db.Query(@"
                                select * from FactMetrics fm
                                where fm.Moduleid = @moduleId and fm.NamespaceId is null and fm.TypeId is null and fm.MemberId is null",
                                new {moduleId}).Single();
                Assert.That(moduleMetrics.MaintainabilityIndex, Is.EqualTo(85));
                Assert.That(moduleMetrics.CodeCoverage, Is.EqualTo(70));
            }
        }

        [Test]
        public void Loader_LoadMetricsAndCodeCoverageAdHoc()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {
                var loader = new Loader(context, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXmlFromAbsolutePath(@"C:\My\CodeMetrics\CodeCoverage\PowerToolOutput\Approve.Me.Common.metrics.xml");
                XElement codeCoverage = UnitTests.LoaderTests.LoadXmlFromAbsolutePath(@"C:\My\CodeMetrics\CodeCoverage\approve.me.api\Summary.xml");
                loader.Load(metrics, codeCoverage, false);
            }
        }

        [Test]
        public void Loader_CreateReadOrDeleteBranch()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {
                var loader = new Loader(context, new TestLogger());

                // Start fresh
                const string branchName = "demo";
                int branchId = loader.CreateReadOrDeleteBranch(branchName);

                int anotherBranchId = loader.CreateReadOrDeleteBranch(branchName);
                Assert.That(anotherBranchId, Is.EqualTo(branchId));

                for (int i = 1; i <= 15; i++)
                {
                    loader.CreateReadOrDeleteBranch(branchName + i);
                    Thread.Sleep(500);
                }

                Assert.That(context.Branches.Count(), Is.EqualTo(10));
            }
        }
    }
}

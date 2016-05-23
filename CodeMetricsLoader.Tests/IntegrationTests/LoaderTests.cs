using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;

using NUnit.Framework;

using CodeMetricsLoader.Data;
using Dapper;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class LoaderTests
    {
        private string _connectionString;
        private SqlConnection _db;

        [TestFixtureSetUp]
        public void Setup()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["IntegrationTestsDb"].ConnectionString;
            _db = new SqlConnection(_connectionString);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _db.Close();
        }

        [Test]
        public void Loader_Load_CanSaveXml()
        {
            DropAndCreateDatabase();
            using (var repository = new MetricsRepository(_connectionString, new TestLogger()))
            {
                Loader loader = new Loader(repository, new TestLogger());
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
            using (var repository = new MetricsRepository(_connectionString, new TestLogger()))
            {
                Loader loader = new Loader(repository, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml("DupCheck.xml");
                loader.Load(metrics, null, true);
            }

            using (var repository = new MetricsRepository(_connectionString, new TestLogger()))
            {
                Loader loader = new Loader(repository, new TestLogger());

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
            using (var repository = new MetricsRepository(_connectionString, new TestLogger()))
            {
                var loader = new Loader(repository, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml();
                loader.Load(metrics, null, false);
                loader.Load(metrics, null, false);

                // TODO: Assert
            }
        }

        [Test]
        public void Loader_LoadMetricsAndCodeCoverage()
        {
            DropAndCreateDatabase();
            using (var repository = new MetricsRepository(_connectionString, new TestLogger()))
            {
                var loader = new Loader(repository, new TestLogger());

                XElement metrics = UnitTests.LoaderTests.LoadXml("CodeMetricsLoader.metrics.xml");
                XElement codeCoverage = UnitTests.LoaderTests.LoadXml("CodeMetricsLoader.CodeCoverage.xml");
                loader.Load(metrics, codeCoverage, false);
                
                var modules =  _db.Query("select * from DimModule").ToList();
                Assert.That(modules.Count(), Is.EqualTo(1));
                var moduleId = modules.First().ModuleId;

                var namespaces = _db.Query("select * from DimNamespace").ToList();
                Assert.That(namespaces.Count, Is.EqualTo(4));

                var types = _db.Query("select * from DimType").ToList();
                Assert.That(types.Count, Is.EqualTo(66));

                var members = _db.Query("select * from DimMember").ToList();
                Assert.That(members.Count, Is.EqualTo(419));

                var moduleMetrics = _db.Query(@"
                                select * from FactMetrics fm
                                where fm.Moduleid = @moduleId and fm.NamespaceId is null and fm.TypeId is null and fm.MemberId is null",
                    new { moduleId }).Single();
                Assert.That(moduleMetrics.MaintainabilityIndex, Is.EqualTo(85));
                Assert.That(moduleMetrics.CodeCoverage, Is.EqualTo(84));
            }
        }

        [Test]
        public void Loader_LoadMetricsAndCodeCoverageAdHoc()
        {
            DropAndCreateDatabase();
            using (var repository = new MetricsRepository(_connectionString, new TestLogger()))
            {
                var loader = new Loader(repository, new TestLogger());
                XElement metrics = UnitTests.LoaderTests.LoadXmlFromAbsolutePath(@"C:\My\CodeMetrics\CodeCoverage\PowerToolOutput\Approve.Me.Common.metrics.xml");
                XElement codeCoverage = UnitTests.LoaderTests.LoadXmlFromAbsolutePath(@"C:\My\CodeMetrics\CodeCoverage\approve.me.api\Summary.xml");
                loader.Load(metrics, codeCoverage, false);
            }
        }

        public void DropAndCreateDatabase()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {
            }
        }
    }
}

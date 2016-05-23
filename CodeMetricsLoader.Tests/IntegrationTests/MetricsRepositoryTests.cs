using System.Configuration;
using System.Linq;
using System.Threading;
using CodeMetricsLoader.Data;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class MetricsRepositoryTests
    {
        private IMetricsRepository _repository;

        [TestFixtureSetUp]
        public void Setup()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["IntegrationTestsDb"].ConnectionString;
            _repository = new MetricsRepository(connectionString, new TestLogger());
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _repository.Dispose();
        }

        [Test]
        public void Loader_CreateReadOrDeleteBranch()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {

                // Start fresh
                const string branchName = "demo";
                int branchId = _repository.CreateReadOrDeleteBranch(branchName);

                int anotherBranchId = _repository.CreateReadOrDeleteBranch(branchName);
                Assert.That(anotherBranchId, Is.EqualTo(branchId));

                for (int i = 1; i <= 15; i++)
                {
                    _repository.CreateReadOrDeleteBranch(branchName + i);
                    Thread.Sleep(500);
                }

                Assert.That(context.Branches.Count(), Is.EqualTo(10));
            }
        }
    }
}
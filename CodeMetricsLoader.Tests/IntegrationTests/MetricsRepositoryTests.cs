using System.Linq;
using System.Threading;
using CodeMetricsLoader.Data;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class MetricsRepositoryTests
    {
        [Test]
        public void Loader_CreateReadOrDeleteBranch()
        {
            using (LoaderContext context = ContextTests.CreateTestContext(true))
            {
                var repository = new MetricsRepository(context, new TestLogger());

                // Start fresh
                const string branchName = "demo";
                int branchId = repository.CreateReadOrDeleteBranch(branchName);

                int anotherBranchId = repository.CreateReadOrDeleteBranch(branchName);
                Assert.That(anotherBranchId, Is.EqualTo(branchId));

                for (int i = 1; i <= 15; i++)
                {
                    repository.CreateReadOrDeleteBranch(branchName + i);
                    Thread.Sleep(500);
                }

                Assert.That(context.Branches.Count(), Is.EqualTo(10));
            }
        }
    }
}
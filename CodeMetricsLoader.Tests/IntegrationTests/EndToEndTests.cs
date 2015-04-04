using System.IO;
using System.Linq;
using CodeMetricsLoader.Data;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    public class EndToEndTests : IntegrationTestBase
    {
        [Test]
        public void Program_Main_S()
        {
            // Arrange
            var args = new string[]
            {
                "--m",
                GetFilePath("metrics-2.xml"),
                "--c",
                ConnectionString,
                "--t",
                "test"
            };
            DeleteTodaysRuns();

            // Act
            Program.Main(args);

            // Assert
            using (LoaderContext testContext = ContextTests.CreateTestContext())
            {
                Assert.That(testContext.Metrics.Count(), Is.EqualTo(8)); // 1 module + 1 namespace + 3 types + 3 members
                Assert.That(testContext.Members.Count(), Is.EqualTo(2)); // 1 duplcate
                Assert.That(testContext.Types.Count(), Is.EqualTo(3));
                Assert.That(testContext.Namespaces.Count(), Is.EqualTo(1));
                Assert.That(testContext.Modules.Count(), Is.EqualTo(1));

                var module = testContext.Modules.First();
                Assert.That(module.Name, Is.EqualTo("Administration.Service.dll"));
                Assert.That(module.AssemblyVersion, Is.EqualTo("1.0.0.0"));
                Assert.That(module.FileVersion, Is.EqualTo("1.0.0.0"));
                var metricsList = module.Metrics.Where(w => w.Namespace == null && w.Type == null && w.Member == null).ToList();
                Assert.That(metricsList.Count, Is.EqualTo(1));
                var metrics = metricsList[0];
                Assert.That(metrics.MaintainabilityIndex, Is.EqualTo(89));
                Assert.That(metrics.CyclomaticComplexity, Is.EqualTo(10945));
                Assert.That(metrics.ClassCoupling, Is.EqualTo(736));
                Assert.That(metrics.DepthOfInheritance, Is.EqualTo(4));
                Assert.That(metrics.LinesOfCode, Is.EqualTo(21106));

                Assert.That(module.Namespaces.Count, Is.EqualTo(1));
                var ns = module.Namespaces[0];
                Assert.That(ns.Name, Is.EqualTo("Administration.Common.Interfaces"));
                metricsList = ns.Metrics.Where(w => w.Type == null && w.Member == null).ToList();
                Assert.That(metricsList.Count, Is.EqualTo(1));
                metrics = metricsList[0];
                Assert.That(metrics.MaintainabilityIndex, Is.EqualTo(100));
                Assert.That(metrics.CyclomaticComplexity, Is.EqualTo(3));
                Assert.That(metrics.ClassCoupling, Is.EqualTo(3));
                Assert.That(metrics.DepthOfInheritance, Is.EqualTo(0));
                Assert.That(metrics.LinesOfCode, Is.EqualTo(0));

                Assert.That(ns.Types.Count, Is.EqualTo(3));
                var type = ns.Types[0];
                Assert.That(type.Name, Is.EqualTo("IBusinessHandler"));
                metricsList = type.Metrics.Where(w => w.Member == null).ToList();
                Assert.That(metricsList.Count, Is.EqualTo(1));
                metrics = metricsList[0];
                Assert.That(metrics.MaintainabilityIndex, Is.EqualTo(100));
                Assert.That(metrics.CyclomaticComplexity, Is.EqualTo(1));
                Assert.That(metrics.ClassCoupling, Is.EqualTo(2));
                Assert.That(metrics.DepthOfInheritance, Is.EqualTo(0));
                Assert.That(metrics.LinesOfCode, Is.EqualTo(0));

                Assert.That(type.Members.Count, Is.EqualTo(1));
                var member = type.Members[0];
                Assert.That(member.Name, Is.EqualTo("ExecuteAction(IRequest) : IResponse"));
                
                // This member is used in two types, IBusinessHandler and IDomain
                Assert.That(member.Metrics.Count(), Is.EqualTo(2));
                metricsList = member.Metrics.Where(w => w.Type.Name == type.Name).ToList();
                Assert.That(metricsList.Count, Is.EqualTo(1));
                metrics = metricsList[0];
                Assert.That(metrics.MaintainabilityIndex, Is.EqualTo(100));
                Assert.That(metrics.CyclomaticComplexity, Is.EqualTo(1));
                Assert.That(metrics.ClassCoupling, Is.EqualTo(2));
                // TODO: This should be null
                Assert.That(metrics.DepthOfInheritance, Is.EqualTo(0));
                Assert.That(metrics.LinesOfCode, Is.EqualTo(0));
            }
        }

        [Test]
        public void Program_Main_M()
        {
            // Arrange
            var args = new string[]
            {
                "--m",
                GetFilePath("metrics-3.xml"),
                "--c",
                ConnectionString,
                "--t",
                "test"
            };
            DeleteTodaysRuns();

            // Act
            Program.Main(args);

            // Assert
            using (LoaderContext testContext = ContextTests.CreateTestContext())
            {
                Assert.That(testContext.Metrics.Count(), Is.EqualTo(427));
                Assert.That(testContext.Members.Count(), Is.EqualTo(315));
                Assert.That(testContext.Types.Count(), Is.EqualTo(63));
                Assert.That(testContext.Namespaces.Count(), Is.EqualTo(9));
                Assert.That(testContext.Modules.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void Program_Main_L()
        {
            // Arrange
            var args = new string[]
            {
                "--m",
                GetFilePath("metrics.xml"),
                "--c",
                ConnectionString,
                "--t",
                "test"
            };
            DeleteTodaysRuns();

            // Act
            Program.Main(args);
        }
    }
}

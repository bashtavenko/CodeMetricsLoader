using CodeMetricsLoader.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class ContextTests
    {
        private LoaderContext _context;
        private static string _databaseName = "CodeMetricsLoaderWarehouseTEST2";

        public static LoaderContext CreateTestContext()
        {
            return new LoaderContext(_databaseName, new DropCreateDatabaseAlways<LoaderContext>());            
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _context = CreateTestContext();
        }

        [Test]
        public void LoaderContext_SmokeTest_CanInitDatabase()
        {
            var dates = _context.Dates.ToList();
        }

        [Test]
        public void LoaderContext_Save_CanSave()
        {
            var run = new DimRun
            {
                Tag = "prog/master",
                Target = "WebServices.Inbound.dll",
                Module = "WebServices.Inbound",
                ModuleAssemblyVersion = "1.0.0.0",
                ModuleFileVersion = "1.0.0.0",
                Namespace = "WebServices.Inbound",
                Type = "Constants",
                Member = "GetPaymentStatus(GetPaymentStatusRequest) : GetPaymentStatusResponse"
            };

            var date = new DimDate();
            var metrics = new FactMetrics
            {
                MaintainabilityIndex = 94,
                CyclomaticComplexity = 112,
                ClassCoupling = 21,
                DepthOfInheritance = 1,
                LinesOfCode = 112,
                Run = run,
                Date = date
            };               
            _context.Metrics.Add(metrics);
            _context.SaveChanges();
        }
    }
}

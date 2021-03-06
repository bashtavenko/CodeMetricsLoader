﻿using System.Data.Entity;
using System.Linq;

using NUnit.Framework;

using CodeMetricsLoader.Data;


namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class ContextTests
    {
        private LoaderContext _context;
        private static string _databaseName = "CodeMetricsLoaderWarehouseTEST";

        public static LoaderContext CreateTestContext(bool deleteDatabase = false)
        {
            if (deleteDatabase)
            {
                var context = new LoaderContext(_databaseName, new DropCreateDatabaseAlways<LoaderContext>());
                context.Database.Delete();
                return new LoaderContext(_databaseName, new DropCreateDatabaseAlways<LoaderContext>());
            }
            else
            {
                return new LoaderContext(_databaseName, null);    
            }
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _context = CreateTestContext(true);
        }

        [Test]
        public void LoaderContext_SmokeTest_CanInitDatabase()
        {
            var dates = _context.Dates.ToList();
        }

        [Test]
        public void LoaderContext_Save_CanSave()
        {
            var module = new DimModule
            {
                Name = "WebServices.Inbound",
                AssemblyVersion = "1.0.0.0",
                FileVersion = "1.0.0.0",
            };

            _context.Modules.Add(module);
                        
            var date = new DimDate();
            var metrics = new FactMetrics
            {
                MaintainabilityIndex = 94,
                CyclomaticComplexity = 112,
                ClassCoupling = 21,
                DepthOfInheritance = 1,
                LinesOfCode = 112,
                Module = module,
                Date = date
            };               
            _context.Metrics.Add(metrics);
            _context.SaveChanges();            
        }
    }
}

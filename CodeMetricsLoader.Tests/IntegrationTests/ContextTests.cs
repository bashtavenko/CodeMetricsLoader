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
            var target = new Target
            { 
                Name = "WebServices.Inbound.dll",
                Modules = new List<Module>
                {
                    new Module
                    {
                         Name = "WebServices.Inbound",
                         AssemblyVersion = "1.0.0.0",
                         FileVersion = "1.0.0.0",
                         Metrics = new Metrics
                         {
                            MaintainabilityIndex = 94,
                            CyclomaticComplexity = 112,
                            ClassCoupling = 21,
                            DepthOfInheritance = 1,
                            LinesOfCode = 112
                         },
                         Namespaces = new List<Namespace>
                         {
                             new Namespace
                             {
                                 Name = "WebServices.Inbound",
                                 Metrics = new Metrics
                                 {
                                    MaintainabilityIndex = 94,
                                    CyclomaticComplexity = 112,
                                    ClassCoupling = 21,
                                    DepthOfInheritance = 1,
                                    LinesOfCode = 112                                    
                                 },
                                 Types = new List<Data.Type>
                                 {
                                     new Data.Type
                                     {
                                         Name = "Constants",
                                         Metrics =  new Metrics
                                         {
                                            MaintainabilityIndex = 94,
                                            CyclomaticComplexity = 112,
                                            ClassCoupling = 21,
                                            DepthOfInheritance = 1,
                                            LinesOfCode = 112                                            
                                         },
                                         Members = new List<Member>
                                         {
                                             new Member
                                             {
                                                 Name = "GetPaymentStatus(GetPaymentStatusRequest) : GetPaymentStatusResponse",
                                                 Metrics =  new Metrics
                                                 {
                                                    MaintainabilityIndex = 94,
                                                    CyclomaticComplexity = 112,
                                                    ClassCoupling = 21,
                                                    DepthOfInheritance = 1,
                                                    LinesOfCode = 112                                                    
                                                 },                                                 
                                             }
                                         }
                                     },                                     
                                 }
                             }
                         }
                    }
                }
            };

            var date = new Date();
            date.Targets.Add(target);
            _context.SaveChanges();
        }
    }
}

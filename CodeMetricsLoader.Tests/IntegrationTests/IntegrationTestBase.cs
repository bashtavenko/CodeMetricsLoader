using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    public class IntegrationTestBase
    {
        private SqlConnection _db;

        public string ConnectionString { get
        {
            return ConfigurationManager.ConnectionStrings["IntegrationTestsDb"].ConnectionString;
        }}

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            string connectionString;
            using (var context = ContextTests.CreateTestContext())
            {
                connectionString = context.Database.Connection.ConnectionString;
            }
            
            _db = new SqlConnection(connectionString);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _db.Close();
        }

        protected void DeleteTodaysRuns()
        {
            var today = DateTime.Now.Date;
            _db.Execute(@"delete from FactMetrics
                          from  factmetrics fm
                          join DimDate dd on dd.DateId = fm.DateId
                          where dd.Date = @date;", new {date = today});

            _db.Execute(@"delete from dimmember
                          from dimmember dm
                          left outer join factmetrics fm on fm.memberid = dm.memberid
                          where fm.memberid is null

                          delete from dimtype 
                          from dimtype dt
                          left outer join factmetrics fm on fm.typeid = dt.typeid
                          where fm.typeid is null

                          delete from dimnamespace
                          from dimnamespace ns
                          left outer join factmetrics fm on fm.namespaceid = ns.namespaceid
                          where fm.namespaceid is null

                          delete from dimmodule
                          from dimmodule dm
                          left outer join factmetrics fm on fm.moduleid = dm.moduleid
                          where fm.moduleid is null");
        }

        protected string GetFilePath (string fileName)
        {
            return Path.Combine("..\\..\\TestFiles\\", fileName);
        }
    }
}

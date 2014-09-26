using CodeMetricsLoader.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeMetricsLoader.Tests.IntegrationTests
{
    [TestFixture]
    public class LoaderTests
    {        
        private Loader _loader;

        [TestFixtureSetUp]
        public void Setup()
        {
            LoaderContext context = ContextTests.CreateTestContext();
            _loader = new Loader(context, new TestLogger());
        }

        [Test]
        public void Loader_SmokeTest_CanSaveXml()
        {
            XElement metrics = UnitTests.LoaderTests.LoadXml();
            _loader.Load(metrics, "master");
        }

    }
}

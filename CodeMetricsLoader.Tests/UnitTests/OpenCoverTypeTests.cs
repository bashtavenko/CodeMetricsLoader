using System;
using CodeMetricsLoader.CodeCoverage;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.UnitTests
{
    public class OpenCoverTypeTests
    {
        [TestCase("Data.StoreContext", "Data", "StoreContext")]
        [TestCase("MyCompany.BusinessLayer.Data.StoreContext", "MyCompany.BusinessLayer.Data", "StoreContext")]
        public void NamespaceMember_Regular(string input, string ns, string member)
        {
            OpenCoverType result = OpenCoverType.Parse(input);
            Assert.AreEqual(ns, result.Namespace);
            Assert.AreEqual(member, result.Member);
        }

        [TestCase("System.Collections.Generic.IList`1<ViewModels.FinanceOption>", "System.Collections.Generic", "IList", "ViewModels.FinanceOption")]
        public void NamespaceMember_Generic(string input, string ns, string member, string memberType)
        {
            OpenCoverType result = OpenCoverType.Parse(input);
            Assert.IsTrue(result.IsGeneric);
            Assert.AreEqual(ns, result.Namespace);
            Assert.AreEqual(member, result.Member);
            Assert.AreEqual(memberType, result.MemberType);
        }

        [TestCase("Data.AutoMapperConfig/EnvironmentTypeResolver", "Data", "EnvironmentTypeResolver")]
        public void NamespaceMember_InnerClass(string input, string ns, string member)
        {
            OpenCoverType result = OpenCoverType.Parse(input);
            Assert.AreEqual(ns, result.Namespace);
            Assert.AreEqual(member, result.Member);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void NamespaceMember_Bogus()
        {
            OpenCoverType result = OpenCoverType.Parse(null);
        }
    }
}

using CodeMetricsLoader.CodeCoverage;
using NUnit.Framework;

namespace CodeMetricsLoader.Tests.UnitTests
{
    [TestFixture]
    public class MethodConverterTests
    {
        private MethodConverter _converter;
        
        [TestFixtureSetUp]
        public void Setup()
        {
            _converter = new MethodConverter();
        }


        [TestCase("System.Void Test.AutoMapperConfig::CreateMaps()", "CreateMaps() : void" )]
        [TestCase("System.String Test.AutoMapperConfig::ShortenText(System.String)", "ShortenText(string) : string")]
        [TestCase("System.Boolean ViewModels.ErrorLogItem::IsMore(System.String,System.String)", "IsMore(string, string) : bool")]
        public void MemberConverter_Methods(string input, string expected)
        {
            var result = _converter.Convert(input);
            Assert.AreEqual(result, expected);
        }

        [TestCase("System.String ViewModels.TrafficItem::get_MachineName()", "MachineName.get() : string")]
        [TestCase("System.Void ViewModels.TrafficItem::set_WebServiceTrafficId(System.String)", "WebServiceTrafficId.set(string) : void")]
        public void MemberConverter_Properties(string input, string expected)
        {
            var result = _converter.Convert(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase("System.Collections.Generic.IList`1<ViewModels.FinanceOption> Controllers.Api.Controller::FinanceOptions(System.Int32)", "FinanceOptions(int) : IList<FinanceOption>")]
        public void MemberConverter_Generics(string input, string expected)
        {
            var result = _converter.Convert(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase("System.Void Controllers.Api.StoreController::.ctor(Data.IStoreRepository)", "StoreController(IStoreRepository)")]
        public void MemberConverter_Constructor(string input, string expected)
        {
            var result = _converter.Convert(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase("System.Nullable`1<System.Byte> Store::get_StorePaymentMethodId()", "StorePaymentMethodId.get() : byte?")]
        public void MemberConverter_Nullable(string input, string expected)
        {
            var result = _converter.Convert(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase(null, null)]
        [TestCase("IsMore", "IsMore")]
        [TestCase("System.Void", "void")]
        [TestCase("System.Boolean", "bool")]
        [TestCase("System.String", "string")]
        [TestCase("System.Char", "char")]
        [TestCase("System.Int32", "int")]
        [TestCase("System.Int64", "int")]
        [TestCase("System.Byte", "byte")]
        [TestCase("System.Decimal", "decimal")]
        [TestCase("System.Web.Mvc.ActionResult", "ActionResult")]
        [TestCase("App.ViewModels.FinanceOption", "FinanceOption")]
        [TestCase("System.Collections.Generic.IList`1<ViewModels.FinanceOption>", "IList<FinanceOption>")]
        public void MemberConverter_ConvertType(string input, string expected)
        {
            var result = _converter.ConvertType(input);
            Assert.AreEqual(expected, result);
        }
    }
}
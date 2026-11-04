using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Logic.Services;

namespace SmartRoutine.Tests
{
    [TestClass]
    public class UrlValidationServiceTests
    {
        private readonly UrlValidationService _service = new UrlValidationService();

        [DataTestMethod]
        [DataRow("https://example.com")]
        [DataRow("HTTP://EXAMPLE.COM")]
        [DataRow("ftp://files.example.com")]
        [DataRow("mailto:user@example.com")]
        [DataRow("mailto:")]
        [DataRow("file:///C:/Users/Name/file.txt")]
        [DataRow("  https://example.com  ")]
        public void IsValidUrl_AcceptsSupportedSchemes(string url)
        {
            Assert.IsTrue(_service.IsValidUrl(url));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("example.com")]
        [DataRow("foo bar")]
        [DataRow("gopher://example.com")]
        public void IsValidUrl_RejectsEverythingElse(string url)
        {
            Assert.IsFalse(_service.IsValidUrl(url));
        }

        [DataTestMethod]
        [DataRow("example.com", "https://example.com")]
        [DataRow("www.example.de/pfad?x=1", "https://www.example.de/pfad?x=1")]
        [DataRow("192.168.1.1", "http://192.168.1.1")]
        [DataRow("192.168.1.1:8080", "http://192.168.1.1:8080")]
        [DataRow("user@example.com", "mailto:user@example.com")]
        [DataRow("\"https://example.com\"", "https://example.com")]
        [DataRow("<https://example.com>", "https://example.com")]
        [DataRow("htps://example.com", "https://example.com")]
        [DataRow("htttp://example.com", "http://example.com")]
        [DataRow("http//example.com", "http://example.com")]
        [DataRow("https//example.com", "https://example.com")]
        [DataRow("ttp://example.com", "http://example.com")]
        [DataRow("tps://example.com", "https://example.com")]
        [DataRow("htp://example.com", "http://example.com")]
        public void TryRepairUrl_FixesCommonMistakes(string input, string expected)
        {
            Assert.AreEqual(expected, _service.TryRepairUrl(input));
        }

        [TestMethod]
        public void TryRepairUrl_Empty_ReturnsInput()
        {
            Assert.AreEqual("", _service.TryRepairUrl(""));
            Assert.IsNull(_service.TryRepairUrl(null));
        }

        [TestMethod]
        public void TryRepairUrl_UnrepairableText_IsReturnedTrimmed()
        {
            Assert.AreEqual("foo bar", _service.TryRepairUrl("  foo bar  "));
        }

        [TestMethod]
        public void ValidateAndRepairUrl_ValidUrl_IsReturnedTrimmed()
        {
            var result = _service.ValidateAndRepairUrl("  https://example.com ");

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual("https://example.com", result.RepairedUrl);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateAndRepairUrl_RepairableUrl_IsRepaired()
        {
            var result = _service.ValidateAndRepairUrl("example.com");

            Assert.IsTrue(result.IsValid);
            Assert.AreEqual("https://example.com", result.RepairedUrl);
        }

        [TestMethod]
        public void ValidateAndRepairUrl_Empty_DependsOnAllowEmpty()
        {
            Assert.IsFalse(_service.ValidateAndRepairUrl("").IsValid);
            Assert.IsFalse(_service.ValidateAndRepairUrl(null).IsValid);
            Assert.IsTrue(_service.ValidateAndRepairUrl("", allowEmpty: true).IsValid);
        }

        [TestMethod]
        public void ValidateAndRepairUrl_TextWithSpaces_SaysNoSpaces()
        {
            var result = _service.ValidateAndRepairUrl("foo bar");

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains(result.ErrorMessage, "Leerzeichen");
            Assert.AreEqual("foo bar", result.RepairedUrl);
        }

        [TestMethod]
        public void ValidateAndRepairUrl_Nonsense_ListsExamples()
        {
            var result = _service.ValidateAndRepairUrl("abc");

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains(result.ErrorMessage, "abc");
            StringAssert.Contains(result.ErrorMessage, "https://example.com");
        }
    }
}

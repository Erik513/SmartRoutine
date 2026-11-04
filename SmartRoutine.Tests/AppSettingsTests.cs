using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.UI.Helpers;

namespace SmartRoutine.Tests
{
    [TestClass]
    public class AppSettingsTests
    {
        // Test data are for Debug builds only, so that testing never touches the real database. A Release
        // build (the one that is published) must use the real data. Run "dotnet test -c Release" before a
        // release: then this test checks the published configuration.
        [TestMethod]
        public void UseTestData_IsOnlyOnInDebugBuilds()
        {
#if DEBUG
            Assert.IsTrue(AppSettings.UseTestData);
#else
            Assert.IsFalse(AppSettings.UseTestData);
#endif
        }
    }
}

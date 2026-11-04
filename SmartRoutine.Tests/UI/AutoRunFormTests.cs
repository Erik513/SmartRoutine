using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartRoutine.Data.Models;
using SmartRoutine.UI.Forms;

namespace SmartRoutine.Tests.UI
{
    [TestClass]
    public class AutoRunFormTests
    {
        [TestMethod]
        public void Constructor_ShowsRoutineNameInTitleAndIsCancelable()
        {
            using (AutoRunForm form = new AutoRunForm(new Routine { Name = "Morgen" }, null, null))
            {
                StringAssert.Contains(form.Text, "Morgen");
                Assert.IsTrue(form.Cancelable);
                Assert.IsFalse(form.IsCancelled);
            }
        }

        [TestMethod]
        public void Constructor_WithoutRoutine_DoesNotThrow()
        {
            using (AutoRunForm form = new AutoRunForm(null, null, null))
            {
                Assert.AreEqual(ErikwnkWFUI.Forms.ProgressState.Running, form.State);
            }
        }
    }
}

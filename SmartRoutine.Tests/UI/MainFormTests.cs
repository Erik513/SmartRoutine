using ErikwnkWFUI.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Forms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace SmartRoutine.Tests.UI
{
    [TestClass]
    public class MainFormTests
    {
        private static MainForm CreateForm()
        {
            var service = new Mock<IRoutineService>();
            service.Setup(s => s.GetAllRoutines()).Returns(new List<Routine>());

            return new MainForm(service.Object);
        }

        [TestMethod]
        public void Constructor_WithoutService_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new MainForm(null));
        }

        [TestMethod]
        public void BottomBar_IsAtTheBottomAndHoldsThemeButtonAndLanguageChoice()
        {
            using (MainForm form = CreateForm())
            {
                var bar = (VersionPanel)typeof(MainForm)
                    .GetField("_versionPanel", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(form);

                Assert.IsNotNull(bar);
                Assert.AreEqual(DockStyle.Bottom, bar.Dock);
                Assert.IsTrue(form.ContentPanel.Controls.Contains(bar));
                Assert.IsFalse(bar.UpdateAvailable);

                var left = (Control)bar.Controls[0];
                Assert.AreEqual(2, left.Controls.Count);
                Assert.IsInstanceOfType(left.Controls[0], typeof(Button));
                Assert.IsInstanceOfType(left.Controls[1], typeof(System.Windows.Forms.ComboBox));
            }
        }
    }
}

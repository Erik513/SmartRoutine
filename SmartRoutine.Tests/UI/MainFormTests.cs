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
        public void BottomBar_IsOnlyOnTheRoutineListNotInTheEditor()
        {
            using (MainForm form = CreateForm())
            {
                var routinesView = (Control)typeof(MainForm)
                    .GetField("_routinesView", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(form);
                var editorView = (Control)typeof(MainForm)
                    .GetField("_editorView", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(form);

                Assert.IsTrue(ContainsVersionPanel(routinesView));
                Assert.IsFalse(ContainsVersionPanel(editorView));
            }
        }

        private static bool ContainsVersionPanel(Control root)
        {
            foreach (Control child in root.Controls)
            {
                if (child is VersionPanel || ContainsVersionPanel(child))
                    return true;
            }

            return false;
        }
    }
}

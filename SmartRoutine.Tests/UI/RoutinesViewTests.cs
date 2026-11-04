using ErikwnkWFUI.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SmartRoutine.Tests.UI
{
    [TestClass]
    public class RoutinesViewTests
    {
        private Mock<IRoutineService> _service;
        private List<Routine> _routines;
        private RoutinesViewUC _view;

        [TestInitialize]
        public void Setup()
        {
            _routines = new List<Routine>
            {
                new Routine { Id = "1", Name = "Morgen", Order = 0 },
                new Routine { Id = "2", Name = "Abend", Order = 1 }
            };

            _service = new Mock<IRoutineService>();
            _service.Setup(s => s.GetAllRoutines()).Returns(() => _routines);

            _view = new RoutinesViewUC(_service.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            _view?.Dispose();
            _view = null;
        }

        private T Field<T>(string name)
        {
            return (T)typeof(RoutinesViewUC)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_view);
        }

        private static int NextNumber(params string[] names)
        {
            var routines = names.Select(n => new Routine { Name = n }).ToList();

            MethodInfo method = typeof(RoutinesViewUC).GetMethod(
                "GetNextRoutineNumber",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (int)method.Invoke(null, new object[] { routines });
        }

        // Next routine number

        [TestMethod]
        public void NextNumber_NoRoutines_IsOne()
        {
            Assert.AreEqual(1, NextNumber());
        }

        [TestMethod]
        public void NextNumber_UsesHighestNumberPlusOne_EvenWithGaps()
        {
            Assert.AreEqual(8, NextNumber("Meine Routine 1", "Meine Routine 7", "Meine Routine 3"));
        }

        [TestMethod]
        public void NextNumber_IgnoresOtherNamesAndNamesWithoutNumber()
        {
            Assert.AreEqual(1, NextNumber("Morgen", "Meine Routine", "Meine Routine abc", "Meine Routine 2x"));
        }

        [TestMethod]
        public void NextNumber_NullName_IsIgnored()
        {
            Assert.AreEqual(3, NextNumber(null, "Meine Routine 2"));
        }

        // Loading and selection

        [TestMethod]
        public void Constructor_ListsRoutinesAndNothingIsSelected()
        {
            var list = Field<TitledListBox>("lstRoutines");

            Assert.AreEqual(2, list.Items.Count);
            Assert.AreEqual(-1, list.SelectedIndex);
            Assert.IsFalse(Field<Button>("btnEditRoutine").Enabled);
            Assert.IsFalse(Field<Button>("btnDeleteRoutine").Enabled);
            Assert.IsFalse(Field<Button>("btnStartRoutine").Enabled);
            Assert.IsFalse(Field<Button>("btnStartRoutineAuto").Enabled);
        }

        [TestMethod]
        public void SelectRoutine_EnablesButtonsAndRaisesSelected()
        {
            Routine selected = null;
            _view.RoutineSelected += (s, r) => selected = r;

            _view.SelectRoutine(_routines[1]);

            Assert.AreSame(_routines[1], selected);
            Assert.IsTrue(Field<Button>("btnEditRoutine").Enabled);
            Assert.IsTrue(Field<Button>("btnStartRoutineAuto").Enabled);
        }

        [TestMethod]
        public void LoadRoutines_KeepsTheSelectedRoutine()
        {
            _view.SelectRoutine(_routines[1]);

            _view.LoadRoutines();

            Assert.AreEqual(1, Field<TitledListBox>("lstRoutines").SelectedIndex);
        }

        // Buttons

        [TestMethod]
        public void StartButton_RaisesEventClearsSelectionAndDisablesButtons()
        {
            Routine started = null;
            _view.StartRoutineClicked += (s, r) => started = r;
            _view.SelectRoutine(_routines[0]);

            Field<Button>("btnStartRoutine").PerformClick();

            Assert.AreSame(_routines[0], started);
            Assert.AreEqual(-1, Field<TitledListBox>("lstRoutines").SelectedIndex);
            Assert.IsFalse(Field<Button>("btnStartRoutine").Enabled);
        }

        [TestMethod]
        public void AutoStartButton_RaisesOnlyTheAutoEvent()
        {
            Routine auto = null;
            bool normalRaised = false;
            _view.StartRoutineAutoClicked += (s, r) => auto = r;
            _view.StartRoutineClicked += (s, r) => normalRaised = true;
            _view.SelectRoutine(_routines[0]);

            Field<Button>("btnStartRoutineAuto").PerformClick();

            Assert.AreSame(_routines[0], auto);
            Assert.IsFalse(normalRaised);
        }

        [TestMethod]
        public void StartButton_WithoutSelection_DoesNothing()
        {
            bool raised = false;
            _view.StartRoutineClicked += (s, r) => raised = true;

            Field<Button>("btnStartRoutine").PerformClick();

            Assert.IsFalse(raised);
        }

        [TestMethod]
        public void EditAndDeleteButtons_RaiseEventsForSelectedRoutine()
        {
            Routine edited = null;
            Routine deleted = null;
            _view.EditRoutineClicked += (s, r) => edited = r;
            _view.DeleteRoutineClicked += (s, r) => deleted = r;
            _view.SelectRoutine(_routines[1]);

            Field<Button>("btnEditRoutine").PerformClick();
            Field<Button>("btnDeleteRoutine").PerformClick();

            Assert.AreSame(_routines[1], edited);
            Assert.AreSame(_routines[1], deleted);
        }

        [TestMethod]
        public void NewButton_CreatesRoutineWithNextNumberAndRaisesEvent()
        {
            _routines.Add(new Routine { Id = "3", Name = "Meine Routine 4", Order = 2 });
            var created = new Routine { Id = "9", Name = "Meine Routine 5" };
            _service.Setup(s => s.CreateRoutine("Meine Routine 5")).Returns(created);

            Routine raised = null;
            _view.NewRoutineClicked += (s, r) => raised = r;

            Field<Button>("btnNewRoutine").PerformClick();

            _service.Verify(s => s.CreateRoutine("Meine Routine 5"), Times.Once);
            Assert.AreSame(created, raised);
        }

        // Bottom bar

        [TestMethod]
        public void BottomBar_IsAtTheBottomAndHoldsThemeButtonAndLanguageChoice()
        {
            var bar = _view.VersionBar;

            Assert.IsNotNull(bar);
            Assert.AreEqual(DockStyle.Bottom, bar.Dock);
            Assert.IsTrue(_view.Controls.Contains(bar));
            Assert.IsFalse(bar.UpdateAvailable);

            var left = bar.Controls[0];
            Assert.AreEqual(2, left.Controls.Count);
            Assert.IsInstanceOfType(left.Controls[0], typeof(Button));
            Assert.IsInstanceOfType(left.Controls[1], typeof(System.Windows.Forms.ComboBox));
        }
    }
}

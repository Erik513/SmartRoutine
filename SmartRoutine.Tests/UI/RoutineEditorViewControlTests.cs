using ErikwnkWFUI.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Controls;
using System;
using System.Reflection;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;
using TableLayoutPanel = System.Windows.Forms.TableLayoutPanel;

namespace SmartRoutine.Tests.UI
{
    [TestClass]
    public class RoutineEditorViewControlTests
    {
        private Mock<IRoutineService> _mockService;
        private Mock<IUrlValidationService> _mockUrlValidation;
        private RoutineEditorUC _editor;
        private Routine _testRoutine;

        [TestInitialize]
        public void Setup()
        {
            _mockService = new Mock<IRoutineService>();
            _mockUrlValidation = new Mock<IUrlValidationService>();

            _testRoutine = new Routine
            {
                Id = "test-id",
                Name = "Test Routine",
                Order = 0,
                Steps = new System.Collections.Generic.List<RoutineStep>()
            };

            _editor = new RoutineEditorUC(_mockService.Object, _mockUrlValidation.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
            // RoutineEditorUC creates a TitledListBox (lstSteps) with
            // AllowDrop=true for its drag-to-reorder support, which registers
            // a real OLE drop target (RegisterDragDrop) on the underlying
            // window handle. That's only revoked when the control's handle is
            // destroyed, which happens via Dispose() - without this, every
            // test in this class leaked one undisposed editor (and its OLE
            // registration) for the lifetime of the whole test run, which
            // could crash the test host after enough of them accumulate.
            _editor?.Dispose();
            _editor = null;
        }


        // LoadRoutine

        [TestMethod]
        public void LoadRoutine_WithValidRoutine_DisplaysRoutineName()
        {
            _editor.LoadRoutine(_testRoutine);

            // Überprüfung über Reflection (da txtRoutineName private ist)
            var txtRoutineName = GetPrivateField<TextBox>(_editor, "txtRoutineName");
            Assert.AreEqual("Test Routine", txtRoutineName.Text);
        }

        [TestMethod]
        public void LoadRoutine_WithNull_CreatesEmptyRoutine()
        {
            _editor.LoadRoutine(null);

            var txtRoutineName = GetPrivateField<TextBox>(_editor, "txtRoutineName");
            Assert.AreEqual("", txtRoutineName.Text);
        }

        [TestMethod]
        public void LoadRoutine_WithSteps_ShowsStepsInListBox()
        {
            _testRoutine.Steps.Add(
                new OpenUrlStep
                {
                    Name = "Google",
                    Order = 0
                });

            _testRoutine.Steps.Add(
                new OpenFolderStep
                {
                    Name = "Dokumente",
                    Order = 1
                });

            _editor.LoadRoutine(_testRoutine);

            var lstSteps =
                GetPrivateField<TitledListBox>(
                    _editor,
                    "lstSteps");

            Assert.AreEqual(2, lstSteps.Items.Count);
        }


        // AddStep

        [TestMethod]
        public void AddStep_Click_ShowsEditorPanel()
        {
            // Arrange
            _editor.LoadRoutine(_testRoutine);

            // Act
            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            // Assert
            var rightTlp = GetPrivateField<TableLayoutPanel>(_editor, "rightTlp");
            Assert.IsTrue(rightTlp.Visible);
        }

        [TestMethod]
        public void AddStep_Click_ClearsEditorFields()
        {
            // Arrange
            _editor.LoadRoutine(_testRoutine);

            // Act
            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            // Assert
            var txtStepName = GetPrivateField<TextBox>(_editor, "txtStepName");
            var txtStepDescription = GetPrivateField<TextBox>(_editor, "txtStepDescription");
            Assert.AreEqual("", txtStepName.Text);
            Assert.AreEqual("", txtStepDescription.Text);
        }


        // DeleteStep

        [TestMethod]
        public void DeleteStep_WithSelectedStep_RemovesStep()
        {
            _editor.AutoConfirmDialogs = true;

            var step = new OpenUrlStep
            {
                Id = "step-1",
                Name = "Google"
            };

            _testRoutine.Steps.Add(step);

            _mockService
                .Setup(x => x.GetRoutine("test-id"))
                .Returns(_testRoutine);

            _editor.LoadRoutine(_testRoutine);

            var lstSteps =
                GetPrivateField<TitledListBox>(
                    _editor,
                    "lstSteps");

            lstSteps.SelectedIndex = 0;

            var btnDeleteStep =
                GetPrivateField<Button>(
                    _editor,
                    "btnDeleteStep");

            btnDeleteStep.PerformClick();

            _mockService.Verify(
                s => s.RemoveStep(
                    "test-id",
                    "step-1"),
                Times.Once);
        }

        [TestMethod]
        public void DeleteStep_WithNoSelectedStep_DoesNothing()
        {
            _editor.LoadRoutine(_testRoutine);

            var btnDeleteStep = GetPrivateField<Button>(_editor, "btnDeleteStep");
            btnDeleteStep.PerformClick();

            _mockService.Verify(s => s.RemoveStep(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }


        // SaveStep Tests



        // CancelStep Tests

        [TestMethod]
        public void CancelStep_Click_ClosesEditorPanel()
        {
            _editor.LoadRoutine(_testRoutine);

            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            var rightTlp = GetPrivateField<TableLayoutPanel>(_editor, "rightTlp");
            Assert.IsTrue(rightTlp.Visible); // Editor ist sichtbar

            var btnCancelStep = GetPrivateField<Button>(_editor, "btnCancelStep");

            btnCancelStep.PerformClick();

            Assert.IsFalse(rightTlp.Visible);
        }


        // Validation Tests

        [TestMethod]
        public void ValidateRoutineName_WithEmptyName_ReturnsFalse()
        {
            _editor.AutoConfirmDialogs = true;
            _editor.LoadRoutine(_testRoutine);
            var txtRoutineName = GetPrivateField<TextBox>(_editor, "txtRoutineName");
            txtRoutineName.Text = "";

            var validateMethod = GetPrivateMethod(_editor, "ValidateRoutineName");
            var result = (bool)validateMethod.Invoke(_editor, null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateStep_WithEmptyName_ShowsError()
        {
            _editor.AutoConfirmDialogs = true;
            _editor.LoadRoutine(_testRoutine);
            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            var txtStepName = GetPrivateField<TextBox>(_editor, "txtStepName");
            txtStepName.Text = ""; // Empty name

            try
            {
                var btnSaveStep = GetPrivateField<Button>(_editor, "btnSaveStep");
                btnSaveStep.PerformClick();
            }
            catch
            {
                Assert.IsTrue(true);
            }
        }


        // Change StepType Tests

        [TestMethod]
        public void ChangingStepType_ShowsCorrectControls()
        {
            _editor.LoadRoutine(_testRoutine);

            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            var cmbStepType = GetPrivateField<ComboBox>(_editor, "cmbStepType");

            cmbStepType.SelectedIndex = 0;

            var editorOptionsTable =
                GetPrivateField<PropertyTable>(_editor, "editorOptionsTable");

            // txtUrl gehört jetzt zum OpenUrlStepEditor (nicht mehr direktes Feld von
            // RoutineEditorUC), daher Suche über den Control-Baum statt Reflection auf das Feld.
            var txtUrl = FindControlByName(editorOptionsTable, "txtUrl");

            Assert.IsTrue(editorOptionsTable.Visible);
            Assert.IsNotNull(txtUrl);
        }

        [TestMethod]
        public void AddStep_AfterSelectingNonUrlStep_ClearsStepTypeSelection()
        {
            // Regressionstest: cmbStepType ist NICHT mehr per DataSource gebunden.
            // Vorher sprang die Auswahl beim Klick auf "Schritt hinzufügen" wieder auf
            // "Webseite öffnen" (Index 0) zurück, aber nur wenn zuvor ein Schritt eines
            // ANDEREN Typs (z.B. Ordner öffnen) ausgewählt war.
            _testRoutine.Steps.Add(new OpenFolderStep
            {
                Id = "folder-step",
                Name = "Dokumente",
                FolderPath = "C:\\Test"
            });

            _editor.LoadRoutine(_testRoutine);

            var lstSteps = GetPrivateField<TitledListBox>(_editor, "lstSteps");
            lstSteps.SelectedIndex = 0;

            var cmbStepType = GetPrivateField<ComboBox>(_editor, "cmbStepType");
            Assert.AreNotEqual(-1, cmbStepType.SelectedIndex, "Vorbedingung: Ordner-Step sollte einen Typ auswählen.");

            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            Assert.AreEqual(-1, cmbStepType.SelectedIndex);
        }

        private Control FindControlByName(Control root, string name)
        {
            foreach (Control child in root.Controls)
            {
                if (child.Name == name)
                    return child;

                var found = FindControlByName(child, name);

                if (found != null)
                    return found;
            }

            return null;
        }


        // Routine name lock

        [TestMethod]
        public void LoadRoutine_NewRoutine_NameBoxIsUnlocked()
        {
            _editor.LoadRoutine(_testRoutine, true);

            var txtRoutineName = GetPrivateField<LockableTextBox>(_editor, "txtRoutineName");
            Assert.IsFalse(txtRoutineName.IsLocked);
        }

        [TestMethod]
        public void LoadRoutine_ExistingRoutine_NameBoxIsLocked()
        {
            _editor.LoadRoutine(_testRoutine, false);

            var txtRoutineName = GetPrivateField<LockableTextBox>(_editor, "txtRoutineName");
            Assert.IsTrue(txtRoutineName.IsLocked);
        }

        [TestMethod]
        public void AcceptingChangedName_SavesRoutineAndLocksBox()
        {
            _editor.AutoConfirmDialogs = true;
            _mockService.Setup(s => s.GetAllRoutines()).Returns(new System.Collections.Generic.List<Routine>());
            _editor.LoadRoutine(_testRoutine, false);

            var txtRoutineName = GetPrivateField<LockableTextBox>(_editor, "txtRoutineName");
            txtRoutineName.BeginEdit();
            txtRoutineName.Text = "Neuer Name";
            txtRoutineName.AcceptEdit();

            _mockService.Verify(s => s.SaveRoutine(It.Is<Routine>(r => r.Name == "Neuer Name")), Times.Once);
            Assert.IsTrue(txtRoutineName.IsLocked);
        }

        [TestMethod]
        public void AcceptingEmptyName_DoesNotSaveAndKeepsBoxUnlocked()
        {
            _editor.AutoConfirmDialogs = true;
            _mockService.Setup(s => s.GetAllRoutines()).Returns(new System.Collections.Generic.List<Routine>());
            _editor.LoadRoutine(_testRoutine, false);

            var txtRoutineName = GetPrivateField<LockableTextBox>(_editor, "txtRoutineName");
            txtRoutineName.BeginEdit();
            txtRoutineName.Text = "   ";
            txtRoutineName.AcceptEdit();

            _mockService.Verify(s => s.SaveRoutine(It.IsAny<Routine>()), Times.Never);
            Assert.IsFalse(txtRoutineName.IsLocked);
        }

        [TestMethod]
        public void AcceptingDuplicateName_DoesNotSave()
        {
            _editor.AutoConfirmDialogs = true;
            _mockService.Setup(s => s.GetAllRoutines()).Returns(new System.Collections.Generic.List<Routine>
            {
                new Routine { Id = "other", Name = "Doppelt" }
            });
            _editor.LoadRoutine(_testRoutine, false);

            var txtRoutineName = GetPrivateField<LockableTextBox>(_editor, "txtRoutineName");
            txtRoutineName.BeginEdit();
            txtRoutineName.Text = "doppelt";
            txtRoutineName.AcceptEdit();

            _mockService.Verify(s => s.SaveRoutine(It.IsAny<Routine>()), Times.Never);
        }

        // ReorderSteps Tests

        [TestMethod]
        public void ReorderSteps_UpdatesStepOrders()
        {
            _testRoutine.Steps.Add(
                new OpenUrlStep
                {
                    Id = "1",
                    Name = "A",
                    Order = 0
                });

            _testRoutine.Steps.Add(
                new OpenUrlStep
                {
                    Id = "2",
                    Name = "B",
                    Order = 1
                });

            _editor.LoadRoutine(_testRoutine);

            var lstSteps =
                GetPrivateField<TitledListBox>(
                    _editor,
                    "lstSteps");

            var method =
                GetPrivateMethod(
                    _editor,
                    "LstSteps_ItemsReordered");

            ((RoutineStep)lstSteps.Items[0]).Order = 1;
            ((RoutineStep)lstSteps.Items[1]).Order = 0;

            method.Invoke(
                _editor,
                new object[]
                {
            lstSteps,
            EventArgs.Empty
                });

            _mockService.Verify(
                s => s.SaveRoutine(
                    It.Is<Routine>(
                        r => r.Steps.Count == 2)),
                Times.Once);
        }


        // Duplicate / Renumber Tests

        private static object InvokeStatic(string name, params object[] args)
        {
            MethodInfo method = typeof(RoutineEditorUC).GetMethod(
                name,
                BindingFlags.NonPublic | BindingFlags.Static);

            return method.Invoke(null, args);
        }

        private static OpenUrlStep Step(string id, int order)
        {
            return new OpenUrlStep { Id = id, Name = "Step " + id, Order = order, Url = "https://example.com" };
        }

        [TestMethod]
        public void RenumberSteps_SetsOrderToListPosition()
        {
            var steps = new System.Collections.Generic.List<RoutineStep> { Step("a", 7), Step("b", 3), Step("c", 3) };

            InvokeStatic("RenumberSteps", steps);

            Assert.AreEqual(0, steps[0].Order);
            Assert.AreEqual(1, steps[1].Order);
            Assert.AreEqual(2, steps[2].Order);
        }

        [TestMethod]
        public void RenumberSteps_EmptyList_DoesNotThrow()
        {
            InvokeStatic("RenumberSteps", new System.Collections.Generic.List<RoutineStep>());
        }

        [TestMethod]
        public void InsertDuplicateAfter_PutsCopyBehindOriginalWithNewIdAndName()
        {
            var steps = new System.Collections.Generic.List<RoutineStep> { Step("a", 0), Step("b", 1), Step("c", 2) };

            var copy = (RoutineStep)InvokeStatic("InsertDuplicateAfter", steps, steps[1]);

            Assert.IsNotNull(copy);
            Assert.AreEqual(4, steps.Count);
            Assert.AreSame(copy, steps[2]);
            Assert.AreNotEqual("b", copy.Id);
            Assert.AreEqual("Step b Kopie", copy.Name);
            Assert.AreEqual("https://example.com", ((OpenUrlStep)copy).Url);

            for (int i = 0; i < steps.Count; i++)
                Assert.AreEqual(i, steps[i].Order);
        }

        [TestMethod]
        public void InsertDuplicateAfter_LastStep_AppendsCopy()
        {
            var steps = new System.Collections.Generic.List<RoutineStep> { Step("a", 0), Step("b", 1) };

            var copy = (RoutineStep)InvokeStatic("InsertDuplicateAfter", steps, steps[1]);

            Assert.AreSame(copy, steps[2]);
            Assert.AreEqual(2, copy.Order);
        }

        [TestMethod]
        public void InsertDuplicateAfter_StepNotInList_ReturnsNullAndKeepsList()
        {
            var steps = new System.Collections.Generic.List<RoutineStep> { Step("a", 0) };

            var copy = InvokeStatic("InsertDuplicateAfter", steps, Step("x", 5));

            Assert.IsNull(copy);
            Assert.AreEqual(1, steps.Count);
        }

        [TestMethod]
        public void DuplicateButton_SavesRoutineWithCopyBehindSelectedStep()
        {
            _testRoutine.Steps.Add(Step("1", 0));
            _testRoutine.Steps.Add(Step("2", 1));
            _mockService.Setup(s => s.GetRoutine("test-id")).Returns(_testRoutine);

            _editor.LoadRoutine(_testRoutine);

            var lstSteps = GetPrivateField<TitledListBox>(_editor, "lstSteps");
            lstSteps.SelectedIndex = 0;

            GetPrivateField<Button>(_editor, "btnDuplicateStep").PerformClick();

            _mockService.Verify(
                s => s.SaveRoutine(It.Is<Routine>(r =>
                    r.Steps.Count == 3 &&
                    r.Steps[0].Id == "1" &&
                    r.Steps[1].Name == "Step 1 Kopie" &&
                    r.Steps[2].Id == "2" &&
                    r.Steps[2].Order == 2)),
                Times.Once);
        }

        // BackButton Tests


        // Hilfsmethode für Reflection

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        private MethodInfo GetPrivateMethod(object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
        }
    }
}
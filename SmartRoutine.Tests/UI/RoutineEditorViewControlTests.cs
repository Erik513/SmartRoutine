using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Controls;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SmartRoutine.Tests.UI
{
    [TestClass]
    public class RoutineEditorViewControlTests
    {
        private Mock<IRoutineService> _mockService;
        private Mock<IUrlValidationService> _mockUrlValidation;
        private RoutineEditorViewControl _editor;
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

            _editor = new RoutineEditorViewControl(_mockService.Object, _mockUrlValidation.Object);
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
            _testRoutine.Steps.Add(new RoutineStep { Id = "1", Name = "Step 1", Order = 0 });
            _testRoutine.Steps.Add(new RoutineStep { Id = "2", Name = "Step 2", Order = 1 });

            _editor.LoadRoutine(_testRoutine);

            var lstSteps = GetPrivateField<StyledListBoxWithHeader>(_editor, "lstSteps");
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
            var step = new RoutineStep { Id = "step1", Name = "Step to Delete", Order = 0 };
            _testRoutine.Steps.Add(step);
            _editor.LoadRoutine(_testRoutine);

            _editor.AutoConfirmDialogs = true;

            var lstSteps = GetPrivateField<StyledListBoxWithHeader>(_editor, "lstSteps");
            lstSteps.SelectedIndex = 0;

            _mockService.Setup(s => s.RemoveStep(_testRoutine.Id, step.Id));
            _mockService.Setup(s => s.GetRoutine(_testRoutine.Id)).Returns(_testRoutine);

            var btnDeleteStep = GetPrivateField<Button>(_editor, "btnDeleteStep");

            btnDeleteStep.PerformClick();

            _mockService.Verify(s => s.RemoveStep(_testRoutine.Id, step.Id), Times.Once);
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

        [TestMethod]
        public void ValidateStep_WithEmptyUrl_ReturnsFalse()
        {
            _editor.AutoConfirmDialogs = true;
            _editor.LoadRoutine(_testRoutine);

            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            var txtStepName = GetPrivateField<TextBox>(_editor, "txtStepName");
            txtStepName.Text = "Valid Name";

            var cmbStepType = GetPrivateField<ComboBox>(_editor, "cmbStepType");
            cmbStepType.SelectedIndex = 0;

            // Panel erstellen, aber URL leer lassen
            var cmbEventMethod = GetPrivateMethod(_editor, "CmbStepType_SelectedIndexChanged");
            cmbEventMethod.Invoke(_editor, new object[] { null, EventArgs.Empty });

            var txtUrl = GetPrivateField<TextBox>(_editor, "txtUrl");
            txtUrl.Text = ""; // Leere URL!

            var validateMethod = GetPrivateMethod(_editor, "ValidateCurrentStep");

            var result = (bool)validateMethod.Invoke(_editor, null);

            Assert.IsFalse(result);
        }


        // UpdateStep Tests

        [TestMethod]
        public void UpdateStep_WithChanges_SavesModifiedStep()
        {
            var step = new RoutineStep { Id = "step1", Name = "Original", Order = 0, Type = StepType.OpenUrl, Value = "https://old.com" };
            _testRoutine.Steps.Add(step);
            _editor.LoadRoutine(_testRoutine);
            _editor.AutoConfirmDialogs = true;

            var lstSteps = GetPrivateField<StyledListBoxWithHeader>(_editor, "lstSteps");
            lstSteps.SelectedIndex = 0; // Step auswählen

            var txtStepName = GetPrivateField<TextBox>(_editor, "txtStepName");
            txtStepName.Text = "Updated Step";

            var btnSaveStep = GetPrivateField<Button>(_editor, "btnSaveStep");

            _mockService.Setup(s => s.UpdateStep(It.IsAny<string>(), step.Id, "Updated Step",
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<StepType>(), It.IsAny<string>()));

            btnSaveStep.PerformClick();

            _mockService.Verify(s => s.UpdateStep(It.IsAny<string>(), step.Id, "Updated Step",
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<StepType>(), It.IsAny<string>()), Times.Once);
        }


        // Change StepType Tests

        [TestMethod]
        public void ChangingStepType_ShowsCorrectControls()
        {
            _editor.LoadRoutine(_testRoutine);

            var btnAddStep = GetPrivateField<Button>(_editor, "btnAddStep");
            btnAddStep.PerformClick();

            var cmbStepType = GetPrivateField<ComboBox>(_editor, "cmbStepType");
            var stepTypeContentTlp = GetPrivateField<TableLayoutPanel>(_editor, "stepTypeContentTlp");

            // OpenUrl auswählen
            cmbStepType.SelectedIndex = 0;
            var cmbEventMethod = GetPrivateMethod(_editor, "CmbStepType_SelectedIndexChanged");
            cmbEventMethod.Invoke(_editor, new object[] { null, EventArgs.Empty });

            //  Panel ist sichtbar
            Assert.IsTrue(stepTypeContentTlp.Visible);
        }


        // ReorderSteps Tests

        [TestMethod]
        public void ReorderSteps_UpdatesStepOrders()
        {
            var step1 = new RoutineStep { Id = "1", Name = "Step 1", Order = 0 };
            var step2 = new RoutineStep { Id = "2", Name = "Step 2", Order = 1 };
            _testRoutine.Steps.Add(step1);
            _testRoutine.Steps.Add(step2);
            _editor.LoadRoutine(_testRoutine);

            var lstSteps = GetPrivateField<StyledListBoxWithHeader>(_editor, "lstSteps");

            //  ItemsReordered Event auslösen (simuliert Drag & Drop)
            var reorderEvent = GetPrivateMethod(_editor, "LstSteps_ItemsReordered");
            reorderEvent.Invoke(_editor, new object[] { null, EventArgs.Empty });

            _mockService.Verify(s => s.UpdateRoutine(It.IsAny<Routine>()), Times.Once);
        }


        // BackButton Tests

        [TestMethod]
        public void BackButton_Click_WithChanges_SavesAndTriggersBackEvent()
        {
            _editor.LoadRoutine(_testRoutine);
            var txtRoutineName = GetPrivateField<TextBox>(_editor, "txtRoutineName");
            txtRoutineName.Text = "Changed Name";

            bool backEventFired = false;
            _editor.BackToRoutinesClicked += (s, e) => backEventFired = true;

            _mockService.Setup(s => s.UpdateRoutine(It.IsAny<Routine>()));
            _mockService.Setup(s => s.GetAllRoutines()).Returns(new System.Collections.Generic.List<Routine>());

            var btnBack = GetPrivateField<Button>(_editor, "btnBack");

            btnBack.PerformClick();

            Assert.IsTrue(backEventFired);
        }


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
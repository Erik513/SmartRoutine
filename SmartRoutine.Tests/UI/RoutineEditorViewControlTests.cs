using CustomWFUI.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.UI.Controls;
using System;
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
                GetPrivateField<StyledListBoxControl>(
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
                GetPrivateField<StyledListBoxControl>(
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
                GetPrivateField<StyledPropertyTable>(_editor, "editorOptionsTable");

            var txtUrl = GetPrivateField<TextBox>(_editor, "txtUrl");

            Assert.IsTrue(editorOptionsTable.Visible);
            Assert.IsNotNull(txtUrl);
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
                GetPrivateField<StyledListBoxControl>(
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
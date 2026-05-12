using SmartRoutine.Data.Models;
using SmartRoutine.Logic.Interfaces;
using SmartRoutine.Logic.Services;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls
{
    public partial class RoutineEditorViewControl : UserControl
    {
        // Für Tests: Wenn true, werden MessageBoxen automatisch mit "Ja" beantwortet
        public bool AutoConfirmDialogs { get; set; } = false;

        public event EventHandler BackToRoutinesClicked;
        public event EventHandler<Routine> SaveChanges;

        private readonly IRoutineService _routineService;
        private readonly IUrlValidationService _urlValidationService;
        private Routine _originalRoutine;
        private Routine _currentRoutine;
        private RoutineStep _editingStep;
        private ToolTip _errorToolTip = UIStyles.ToolTips.CreateToolTip();
        private bool _isRefreshing = false;

        // UI Controls
        private TableLayoutPanel mainTlp;

        // Inhalt (links + rechts)
        private TableLayoutPanel contentTlp;

        // Linke Seite
        private TableLayoutPanel leftTlp;
        private TableLayoutPanel leftRoutineTitleTlp;
        private Label lblRoutineName;
        private TextBox txtRoutineName;
        private StyledListBoxWithHeader lstSteps;
        //private ListBox lstSteps;

        private TableLayoutPanel leftBtnsTlp;
        private Button btnAddStep, btnDeleteStep;

        // Rechte Seite
        private TableLayoutPanel rightTlp;
        private TableLayoutPanel rightTitleTlp;
        private Label lblStepNameTitle;
        private ToggleSwitch tglStepEnabled;
        private Label lblStepName;
        private TextBox txtStepName;
        private Label lblStepDescription;
        private TextBox txtStepDescription;
        private Label lblStepType;
        private ComboBox cmbStepType;

        private TableLayoutPanel rightBtnsTlp;
        private Button btnSaveStep, btnCancelStep;

        // Controls based on StepType
        // StepTypeContent
        private Panel stepTypeContentPanel;
        private TableLayoutPanel stepTypeContentTlp;
        
        // OpenURL Controls
        private TextBox txtUrl;
        private ToggleSwitch chkOpenInExternBrowser;

        // OpenFolder Controls
        private TextBox txtFolderPath;
        private ToggleSwitch chkOpenInNewWindow;

        // OpenApplication Controls
        private TextBox txtAppPath;
        private TextBox txtAppArguments;
        private ToggleSwitch chkRunAsAdmin;

        // Footer
        private Button btnBack;

        public RoutineEditorViewControl(IRoutineService routineService, IUrlValidationService urlValidationService)
        {
            _routineService = routineService;
            _urlValidationService = urlValidationService;
            _originalRoutine = null;
            _currentRoutine = null;
            _editingStep = null;

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Black;

            this.Load += (s, e) =>
            {
                lstSteps.Visible = true;
                lstSteps.Invalidate();
                lstSteps.Update();
            };

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            InitializeControl();
        }

        private void InitializeControl()
        {
            // Hauptstruktur
            mainTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 2);
            mainTlp.Dock = DockStyle.Fill;
            mainTlp.RowStyles.Clear();
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Inhalt
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Footer

            // ========== INHALT (Linke + Rechte Seite) ==========
            contentTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            contentTlp.Dock = DockStyle.Fill;
            contentTlp.ColumnStyles.Clear();
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));  // Linke Seite 40%
            contentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));  // Rechte Seite 60%

            // ----- LINKE SEITE -----
            leftTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 3);
            leftTlp.Dock = DockStyle.Fill;
            leftTlp.RowStyles.Clear();
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            leftTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Oben
            leftRoutineTitleTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            leftRoutineTitleTlp.Dock = DockStyle.Fill;
            leftRoutineTitleTlp.ColumnStyles.Clear();
            leftRoutineTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); 
            leftRoutineTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            lblRoutineName = UIStyles.Labels.CreateTitle("Routinenname:");
            lblRoutineName.Dock = DockStyle.Fill;

            txtRoutineName = UIStyles.TextBoxes.CreateStandard();
            txtRoutineName.Dock = DockStyle.Fill;

            leftRoutineTitleTlp.Controls.Add(lblRoutineName, 0, 0);
            leftRoutineTitleTlp.Controls.Add(txtRoutineName, 1, 0);

            // Mitte
            lstSteps = new StyledListBoxWithHeader("Schritte", ContentAlignment.MiddleCenter)
            {
                Dock = DockStyle.Fill,
                ItemHeightCustom = 35,
                Visible = true,
            };
            lstSteps.SelectedIndexChanged += LstSteps_SelectedIndexChanged;
            lstSteps.ItemsReordered += LstSteps_ItemsReordered;

            // Unten
            leftBtnsTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            leftBtnsTlp.Dock = DockStyle.Fill;

            btnAddStep = UIStyles.Buttons.CreatePrimary("+ Schritt hinzufügen", "", new Size(155, 35));
            btnAddStep.Click += BtnAddStep_Click;

            btnDeleteStep = UIStyles.Buttons.CreateDanger("🗑 Löschen", "", new Size(155, 35));
            btnDeleteStep.Click += BtnDeleteStep_Click;
            btnDeleteStep.Enabled = false;

            leftBtnsTlp.ColumnStyles.Clear();
            leftBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnAddStep.Width));
            leftBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnDeleteStep.Width));

            leftBtnsTlp.Controls.Add(btnAddStep, 0, 0);
            leftBtnsTlp.Controls.Add(btnDeleteStep, 1, 0);

            leftTlp.Controls.AddRange(new Control[] {
                leftRoutineTitleTlp, lstSteps, leftBtnsTlp
            });

            // ----- RECHTE SEITE -----
            rightTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 9);
            rightTlp.Dock = DockStyle.Fill;
            rightTlp.RowStyles.Clear();
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // lblStepNameTitle
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // lblStepName
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // txtStepName
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // lblStepDescription
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // txtStepDescription
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // lblStepType
            rightTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // cmbStepType
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // stepTypeContentTlp
            rightTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // btnSaveStep, btnCancelStep
            rightTlp.Visible = false;

            // rightTitleTlp
            rightTitleTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            rightTitleTlp.Dock = DockStyle.Fill;
            rightTitleTlp.ColumnStyles.Clear();
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // lblStepNameTitle
            rightTitleTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60)); // chkStepEnabled

            // Titel (lblStepNameTitle)
            lblStepNameTitle = UIStyles.Labels.CreateTitle();
            lblStepNameTitle.Dock = DockStyle.Fill;

            // chkStepEnabled
            tglStepEnabled = UIStyles.ToggleSwitches.CreateStandard(true, "Schritt ist aktiviert", "Schritt ist deaktiviert");
            tglStepEnabled.Location = new Point(0, 0);
            tglStepEnabled.Anchor = AnchorStyles.None;

            rightTitleTlp.Controls.Add(lblStepNameTitle, 0, 0);
            rightTitleTlp.Controls.Add(tglStepEnabled, 1, 0);

            // lblStepName
            lblStepName = UIStyles.Labels.CreateNormal("Name:");
            lblStepName.Dock = DockStyle.Fill;

            // txtStepName
            txtStepName = UIStyles.TextBoxes.CreateStandard();
            txtStepName.Dock = DockStyle.Fill;

            // lblStepDescription
            lblStepDescription = UIStyles.Labels.CreateNormal("Beschreibung:");
            lblStepDescription.Dock = DockStyle.Fill;

            // txtStepDescription
            txtStepDescription = UIStyles.TextBoxes.CreateStandard();
            txtStepDescription.Dock = DockStyle.Fill;

            // lblStepType
            lblStepType = UIStyles.Labels.CreateNormal("Aktion:");
            lblStepType.Dock = DockStyle.Fill;
            // cmbStepType
            cmbStepType = UIStyles.ComboBoxes.CreateStandard(ComboBoxStyle.DropDownList);
            cmbStepType.Dock = DockStyle.Fill;
            var stepTypes = StepTypeHelper.GetStepTypeListWithEmpty();
            cmbStepType.DataSource = stepTypes;
            cmbStepType.DisplayMember = "Value";
            cmbStepType.ValueMember = "Key";
            cmbStepType.SelectedIndex = -1;
            cmbStepType.SelectedIndexChanged += CmbStepType_SelectedIndexChanged;

            // ==================================================================================================================
            // stepTypeContent
            stepTypeContentPanel = UIStyles.Panels.CreateMedium();
            stepTypeContentPanel.Dock = DockStyle.Fill;

            stepTypeContentTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 1);
            stepTypeContentTlp.Dock = DockStyle.Fill;

            stepTypeContentPanel.Controls.Add(stepTypeContentTlp);

            txtUrl = UIStyles.TextBoxes.CreateStandard();
            txtUrl.Dock = DockStyle.Fill;
            txtUrl.TextChanged += TxtUrl_TextChanged;  // Für Live-Validierung
            txtUrl.LostFocus += TxtUrl_LostFocus;       // Für finale Validierung

            // Drag & Drop für txtUrl aktivieren
            DragDropHelper.EnableTextDragDrop(txtUrl, (droppedText) =>
            {
                // Optional: Bereinige den gedroppten Text
                string cleanedText = droppedText.Trim();

                // Setze den Text in die TextBox
                txtUrl.Text = cleanedText;

                // Führe die Validierung aus
                TxtUrl_TextChanged(txtUrl, EventArgs.Empty);

                // Setze den Cursor ans Ende
                txtUrl.SelectionStart = txtUrl.Text.Length;
            });

            // ==================================================================================================================

            // btnSaveStep, btnCancelStep
            rightBtnsTlp = UIStyles.TableLayoutPanels.CreateStandard(2, 1);
            rightBtnsTlp.Dock = DockStyle.Fill;

            btnSaveStep = UIStyles.Buttons.CreatePrimary("💾 Schritt speichern", "", new Size(155, 35));
            btnSaveStep.Click += BtnSaveStep_Click;

            btnCancelStep = UIStyles.Buttons.CreateStandard("✖ Abbrechen", "", new Size(155, 35));
            btnCancelStep.Click += BtnCancelStep_Click;

            rightBtnsTlp.ColumnStyles.Clear();
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnSaveStep.Width));
            rightBtnsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, btnCancelStep.Width));
            rightBtnsTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            rightBtnsTlp.Controls.Add(btnSaveStep, 0, 0);
            rightBtnsTlp.Controls.Add(btnCancelStep, 1, 0);

            rightTlp.Controls.AddRange(new Control[] {
                rightTitleTlp, lblStepName, txtStepName,
                lblStepDescription, txtStepDescription,
                lblStepType, cmbStepType, stepTypeContentPanel, 
                rightBtnsTlp
            });

            // ========== FOOTER ==========
            var footerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIStyles.Colors.BackgroundDark,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnBack = UIStyles.Buttons.CreateStandard("← Zurück", "", new Size(100, 35));
            btnBack.Location = new Point(footerPanel.Width - 120, 12);
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBack.Click += BtnBack_Click;

            footerPanel.Controls.AddRange(new Control[] { btnBack });

            // Hinzufügen
            contentTlp.Controls.Add(leftTlp, 0, 0);
            contentTlp.Controls.Add(rightTlp, 1, 0);

            mainTlp.Controls.Add(contentTlp, 0, 0);
            mainTlp.Controls.Add(footerPanel, 0, 1);

            this.Controls.Add(mainTlp);

            // Initial Zustand
            SetEditorEnabled(false);
        }

        // ========== PUBLIC METHODS ==========
        public void LoadRoutine(Routine routine)
        {
            _originalRoutine = DeepCopy(routine ?? new Routine());
            _currentRoutine = DeepCopy(routine ?? new Routine());

            txtRoutineName.Text = _currentRoutine.Name;

            lstSteps.SelectedIndex = -1;
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;

            RefreshStepsList(silent: true);
            ClearEditor();

            if (_currentRoutine.Steps.Count == 0)
                rightTlp.Visible = false;

            if (routine?.IsNew == true)
            {
                this.BeginInvoke(new Action(() =>
                {
                    txtRoutineName.Focus();
                    txtRoutineName.SelectAll();
                }));

                routine.IsNew = false;
            }
        }
        private void TxtUrl_TextChanged(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                // Leeres Feld - normale Farbe
                txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(txtUrl, "");
                return;
            }

            // Live-Validierung während der Eingabe
            var result = _urlValidationService.ValidateAndRepairUrl(url, false);

            if (result.IsValid)
            {
                // Gültige URL - normale Farbe
                txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(txtUrl, "");

                // Wenn die URL repariert wurde, im Hintergrund merken (aber nicht überschreiben während der Eingabe)
                if (result.RepairedUrl != url && result.RepairedUrl != txtUrl.Tag as string)
                {
                    txtUrl.Tag = result.RepairedUrl; // Reparierte URL als Tag speichern
                }
            }
            else
            {
                // Ungültige URL - rote Farbe und Fehlermeldung im ToolTip
                txtUrl.ForeColor = UIStyles.Colors.Red;
                _errorToolTip.SetToolTip(txtUrl, result.ErrorMessage);
            }
        }
        private void TxtUrl_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUrl.Text))
                return;

            var result = _urlValidationService.ValidateAndRepairUrl(txtUrl.Text, false);

            if (result.IsValid)
            {
                // Verwende die reparierte URL wenn vorhanden
                if (result.RepairedUrl != txtUrl.Text)
                {
                    txtUrl.Text = result.RepairedUrl;
                }
                txtUrl.ForeColor = UIStyles.Colors.TextPrimary;
                _errorToolTip.SetToolTip(txtUrl, "");
            }
            else
            {
                txtUrl.ForeColor = UIStyles.Colors.Red;
                _errorToolTip.SetToolTip(txtUrl, result.ErrorMessage);
            }
        }
        private Routine DeepCopy(Routine original)
        {
            if (original == null) return null;

            var copy = new Routine
            {
                Id = original.Id,
                Name = original.Name,
                Order = original.Order,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                Steps = new List<RoutineStep>()
            };

            foreach (var step in original.Steps)
            {
                switch (step)
                {
                    case OpenUrlStep urlStep:
                        copy.Steps.Add(new OpenUrlStep
                        {
                            Id = urlStep.Id,
                            Order = urlStep.Order,
                            Name = urlStep.Name,
                            Description = urlStep.Description,
                            Show = urlStep.Show,
                            Url = urlStep.Url,
                            OpenInExternBrowser = urlStep.OpenInExternBrowser
                        });
                        break;
                    case OpenFolderStep folderStep:
                        copy.Steps.Add(new OpenFolderStep
                        {
                            Id = folderStep.Id,
                            Order = folderStep.Order,
                            Name = folderStep.Name,
                            Description = folderStep.Description,
                            Show = folderStep.Show,
                            FolderPath = folderStep.FolderPath,
                            OpenInNewWindow = folderStep.OpenInNewWindow
                        });
                        break;
                    case OpenApplicationStep appStep:
                        copy.Steps.Add(new OpenApplicationStep
                        {
                            Id = appStep.Id,
                            Order = appStep.Order,
                            Name = appStep.Name,
                            Description = appStep.Description,
                            Show = appStep.Show,
                            ApplicationPath = appStep.ApplicationPath,
                            Arguments = appStep.Arguments,
                            RunAsAdmin = appStep.RunAsAdmin,
                            WorkingDirectory = appStep.WorkingDirectory
                        });
                        break;
                }
            }

            return copy;
        }

        private bool HasChanges()
        {
            if (_originalRoutine == null)
            {
                return !string.IsNullOrWhiteSpace(txtRoutineName.Text) || _currentRoutine.Steps.Count > 0;
            }

            if (_originalRoutine.Name != txtRoutineName.Text)
                return true;

            if (_originalRoutine.Steps.Count != _currentRoutine.Steps.Count)
                return true;

            for (int i = 0; i < _originalRoutine.Steps.Count; i++)
            {
                var orig = _originalRoutine.Steps[i];
                var curr = _currentRoutine.Steps[i];

                if (orig.Order != curr.Order || orig.Name != curr.Name || orig.Description != curr.Description)
                    return true;

                // Step-spezifische Vergleiche
                switch (orig)
                {
                    case OpenUrlStep origUrl when curr is OpenUrlStep currUrl:
                        if (origUrl.Url != currUrl.Url || origUrl.OpenInExternBrowser != currUrl.OpenInExternBrowser)
                            return true;
                        break;
                    case OpenFolderStep origFolder when curr is OpenFolderStep currFolder:
                        if (origFolder.FolderPath != currFolder.FolderPath || origFolder.OpenInNewWindow != currFolder.OpenInNewWindow)
                            return true;
                        break;
                    case OpenApplicationStep origApp when curr is OpenApplicationStep currApp:
                        if (origApp.ApplicationPath != currApp.ApplicationPath ||
                            origApp.Arguments != currApp.Arguments ||
                            origApp.RunAsAdmin != currApp.RunAsAdmin)
                            return true;
                        break;
                }
            }

            return false;
        }
        
        private void OpenEditorForStep(RoutineStep step = null)
        {
            CloseEditor();
            if (step != null)
            {
                LoadStepToEditor(step);
            }
            else
            {
                ClearEditor();
            }

            rightTlp.Visible = true;
            SetEditorEnabled(true);
        }


        private void CloseEditor()
        {
            rightTlp.Visible = false;
            _editingStep = null;
        }

        private void ClearEditor()
        {
            lblStepNameTitle.Text = "Neuen Schritt erstellen";
            txtStepName.Text = "";
            txtStepDescription.Text = "";

            // OpenUrl Controls
            if (txtUrl != null) txtUrl.Text = "";
            if (chkOpenInExternBrowser != null) chkOpenInExternBrowser.Checked = true;

            // OpenFolder Controls
            if (txtFolderPath != null) txtFolderPath.Text = "";
            if (chkOpenInNewWindow != null) chkOpenInNewWindow.Checked = true;

            // OpenApplication Controls
            if (txtAppPath != null) txtAppPath.Text = "";
            if (txtAppArguments != null) txtAppArguments.Text = "";
            if (chkRunAsAdmin != null) chkRunAsAdmin.Checked = false;

            tglStepEnabled.Checked = true;
            cmbStepType.SelectedIndex = -1;
            stepTypeContentTlp.Visible = false;
            _editingStep = null;
            SetEditorEnabled(false);
        }

        private void LoadStepToEditor(RoutineStep step)
        {
            lblStepNameTitle.Text = $"{step.Name} bearbeiten";
            txtStepName.Text = step.Name;
            txtStepDescription.Text = step.Description;
            tglStepEnabled.Checked = step.Show;

            // StepType auswählen
            for (int i = 0; i < cmbStepType.Items.Count; i++)
            {
                var item = (KeyValuePair<StepType, string>)cmbStepType.Items[i];
                if (item.Key == step.Type)
                {
                    cmbStepType.SelectedIndex = i;
                    break;
                }
            }

            // Step-spezifische Werte laden
            switch (step)
            {
                case OpenUrlStep urlStep:
                    if (txtUrl != null) txtUrl.Text = urlStep.Url;
                    if (chkOpenInExternBrowser != null) chkOpenInExternBrowser.Checked = urlStep.OpenInExternBrowser;
                    break;
                case OpenFolderStep folderStep:
                    if (txtFolderPath != null) txtFolderPath.Text = folderStep.FolderPath;
                    if (chkOpenInNewWindow != null) chkOpenInNewWindow.Checked = folderStep.OpenInNewWindow;
                    break;
                case OpenApplicationStep appStep:
                    if (txtAppPath != null) txtAppPath.Text = appStep.ApplicationPath;
                    if (txtAppArguments != null) txtAppArguments.Text = appStep.Arguments;
                    if (chkRunAsAdmin != null) chkRunAsAdmin.Checked = appStep.RunAsAdmin;
                    break;
            }

            _editingStep = step;
            SetEditorEnabled(true);

            this.BeginInvoke(new Action(() =>
            {
                cmbStepType.PerformLayout();
                cmbStepType.Invalidate();
                cmbStepType.Update();
            }));
        }

        private void SetEditorEnabled(bool enabled)
        {
            foreach (Control control in rightTlp.Controls)
            {
                control.Enabled = enabled;
            }
            // stepTypeContentTlp Controls manuell aktivieren/deaktivieren
            foreach (Control control in stepTypeContentTlp.Controls)
            {
                control.Enabled = enabled;
            }
        }

        // ========== STEP EVENT HANDLER ==========
        private void LstSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isRefreshing) return;

            if (!(lstSteps.SelectedItem is RoutineStep step))
            {
                rightTlp.Visible = false;
                ClearEditor();
                btnDeleteStep.Enabled = false;
                return;
            }

            CloseEditor();
            OpenEditorForStep(step);
            btnDeleteStep.Enabled = true;
        }

        private void LstSteps_ItemsReordered(object sender, EventArgs e)
        {
            if (_isRefreshing) return;

            var reorderedSteps = lstSteps.Items.Cast<RoutineStep>().ToList();

            for (int i = 0; i < reorderedSteps.Count; i++)
            {
                reorderedSteps[i].Order = i;
            }
            _currentRoutine.Steps = reorderedSteps;
            _routineService.UpdateRoutine(_currentRoutine);
            lstSteps.Invalidate();
            lstSteps.Update();
        }
        private void BtnAddStep_Click(object sender, EventArgs e)
        {
            CloseEditor();
            OpenEditorForStep(null);
            txtStepName.Focus();
        }

        private void BtnDeleteStep_Click(object sender, EventArgs e)
        {
            if (!(lstSteps.SelectedItem is RoutineStep stepToDelete)) return;

            bool shouldDelete = AutoConfirmDialogs;

            if (!shouldDelete)
            {
                shouldDelete = MessageBox.Show($"Schritt '{stepToDelete.Name}' wirklich löschen?",
                    "Bestätigen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            }

            if (!shouldDelete) return;

            _routineService.RemoveStep(_currentRoutine.Id, stepToDelete.Id);
            _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

            RefreshStepsList(silent: true);

            lstSteps.SelectedIndex = -1;
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = _currentRoutine.Steps.Count > 0;

            SaveChanges?.Invoke(this, _currentRoutine);
        }



        // ========== STEP LIST METHODS ==========
        private void RefreshStepsList(bool silent = false)
        {
            if (_currentRoutine == null) return;

            _isRefreshing = true;
            lstSteps.BeginUpdate();
            lstSteps.Items.Clear();

            // Stelle sicher, dass die Steps aus der aktuellen Routine kommen
            var orderedSteps = _currentRoutine.Steps.OrderBy(s => s.Order).ToList();

            Debug.WriteLine($"RefreshStepsList: {orderedSteps.Count} Schritte werden geladen");
            foreach (var step in orderedSteps)
            {
                Debug.WriteLine($"  - {step.Name} (Order: {step.Order})");
                lstSteps.Items.Add(step);
            }

            lstSteps.EndUpdate();
            _isRefreshing = false;

            if (lstSteps.Items.Count > 0)
                lstSteps.SelectedIndex = -1;

            if (!silent)
                Debug.WriteLine($"RefreshStepsList: {orderedSteps.Count} Schritte geladen");
        }


        private void BtnSaveStep_Click(object sender, EventArgs e)
        {
            if (!ValidateCurrentStep()) return;
            SaveCurrentStep();
            lstSteps.SelectedIndex = -1;
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = false;
        }

        private void BtnCancelStep_Click(object sender, EventArgs e)
        {
            lstSteps.SelectedIndex = -1;
            ClearEditor();
            rightTlp.Visible = false;
            btnDeleteStep.Enabled = lstSteps.Items.Count > 0 && lstSteps.SelectedIndex >= 0;
        }
        private void CmbStepType_SelectedIndexChanged(object sender, EventArgs e)
        {
            stepTypeContentTlp.Controls.Clear();
            stepTypeContentTlp.RowStyles.Clear();
            stepTypeContentTlp.ColumnStyles.Clear();

            if (cmbStepType.SelectedIndex == -1)
            {
                stepTypeContentTlp.Visible = false;
                return;
            }

            var selectedType = (StepType)cmbStepType.SelectedValue;

            switch (selectedType)
            {
                case StepType.OpenUrl:
                    CreateOpenUrlControls();
                    break;
                case StepType.OpenFolder:
                    CreateOpenFolderControls();
                    break;
                case StepType.OpenApplication:
                    CreateOpenApplicationControls();
                    break;
            }
        }

        private void CreateOpenUrlControls()
        {
            stepTypeContentTlp.Controls.Clear();
            stepTypeContentTlp.RowStyles.Clear();
            stepTypeContentTlp.ColumnCount = 1;
            stepTypeContentTlp.RowCount = 4;  // 4 Zeilen (keine extra Füllzeile mehr)
            stepTypeContentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Label URL
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // TextBox URL
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Horizontale Reihe (Label + Toggle)
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Füll-Zeile

            // Label URL
            var lblUrl = UIStyles.Labels.CreateNormal("URL:");
            lblUrl.Dock = DockStyle.Fill;
            lblUrl.Margin = new Padding(3, 5, 3, 3);

            // TextBox URL
            txtUrl = UIStyles.TextBoxes.CreateStandard();
            txtUrl.Name = "txtUrl";
            txtUrl.Dock = DockStyle.Fill;
            txtUrl.Margin = new Padding(3, 3, 3, 10);

            // Horizontales Panel für Label + ToggleSwitch
            var horizontalPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            horizontalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            horizontalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            horizontalPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Label
            var lblInternal = UIStyles.Labels.CreateNormal("Öffnen in (App/Browser):");
            lblInternal.Dock = DockStyle.Fill;
            lblInternal.Margin = new Padding(3, 5, 3, 3);

            // ToggleSwitch
            chkOpenInExternBrowser = UIStyles.ToggleSwitches.CreateStandard(false, "Externer Browser", "In App öffnen");
            chkOpenInExternBrowser.Name = "chkOpenInternally";
            chkOpenInExternBrowser.Anchor = AnchorStyles.Left;
            chkOpenInExternBrowser.Margin = new Padding(3, 3, 3, 3);

            horizontalPanel.Controls.Add(lblInternal, 0, 0);
            horizontalPanel.Controls.Add(chkOpenInExternBrowser, 1, 0);

            // Controls hinzufügen
            stepTypeContentTlp.Controls.Add(lblUrl, 0, 0);
            stepTypeContentTlp.Controls.Add(txtUrl, 0, 1);
            stepTypeContentTlp.Controls.Add(horizontalPanel, 0, 2);

            stepTypeContentTlp.Visible = true;
        }
        private void CreateOpenFolderControls()
        {
            stepTypeContentTlp.Controls.Clear();
            stepTypeContentTlp.RowStyles.Clear();
            stepTypeContentTlp.ColumnCount = 2;
            stepTypeContentTlp.RowCount = 4;
            stepTypeContentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            stepTypeContentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Label Ordnerpfad
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // TextBox + Browse Button
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Horizontale Reihe (Label + Toggle)
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Füll-Zeile

            // Label Ordnerpfad
            var lblPath = UIStyles.Labels.CreateNormal("Ordnerpfad:");
            lblPath.Dock = DockStyle.Fill;
            lblPath.Margin = new Padding(3, 5, 3, 3);

            // TextBox Ordnerpfad
            txtFolderPath = UIStyles.TextBoxes.CreateStandard();
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.Dock = DockStyle.Fill;
            txtFolderPath.Margin = new Padding(3, 3, 3, 3);

            // Browse Button
            var btnBrowse = UIStyles.Buttons.CreateStandard("Durchsuchen...");
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Margin = new Padding(3, 3, 3, 10);
            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                        txtFolderPath.Text = dialog.SelectedPath;
                }
            };

            // Horizontales Panel für Label + ToggleSwitch
            var horizontalPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            horizontalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            horizontalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            horizontalPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblOptions = UIStyles.Labels.CreateNormal("Im neuen Fenster öffnen:");
            lblOptions.Dock = DockStyle.Fill;
            lblOptions.Margin = new Padding(3, 5, 3, 3);

            chkOpenInNewWindow = UIStyles.ToggleSwitches.CreateStandard(false, "Ja", "Nein");
            chkOpenInNewWindow.Name = "chkOpenInNewWindow";
            chkOpenInNewWindow.Anchor = AnchorStyles.Left;
            chkOpenInNewWindow.Margin = new Padding(3, 3, 3, 3);

            horizontalPanel.Controls.Add(lblOptions, 0, 0);
            horizontalPanel.Controls.Add(chkOpenInNewWindow, 1, 0);

            // Controls hinzufügen
            stepTypeContentTlp.Controls.Add(lblPath, 0, 0);
            stepTypeContentTlp.SetColumnSpan(lblPath, 2);

            stepTypeContentTlp.Controls.Add(txtFolderPath, 0, 1);
            stepTypeContentTlp.Controls.Add(btnBrowse, 1, 1);

            stepTypeContentTlp.Controls.Add(horizontalPanel, 0, 2);
            stepTypeContentTlp.SetColumnSpan(horizontalPanel, 2);

            stepTypeContentTlp.Visible = true;
        }
        private void CreateOpenApplicationControls()
        {
            stepTypeContentTlp.Controls.Clear();
            stepTypeContentTlp.RowStyles.Clear();
            stepTypeContentTlp.ColumnCount = 2;
            stepTypeContentTlp.RowCount = 6;
            stepTypeContentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            stepTypeContentTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Label Programmpfad
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // TextBox + Browse Button
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Label Argumente
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // TextBox Argumente
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Horizontale Reihe (Label + Toggle)
            stepTypeContentTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Füll-Zeile

            // Label Programmpfad
            var lblPath = UIStyles.Labels.CreateNormal("Programmpfad:");
            lblPath.Dock = DockStyle.Fill;
            lblPath.Margin = new Padding(3, 5, 3, 3);

            // TextBox Programmpfad
            txtAppPath = UIStyles.TextBoxes.CreateStandard();
            txtAppPath.Name = "txtAppPath";
            txtAppPath.Dock = DockStyle.Fill;
            txtAppPath.Margin = new Padding(3, 3, 3, 3);

            // Browse Button
            var btnBrowse = UIStyles.Buttons.CreateStandard("Durchsuchen...");
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.Margin = new Padding(3, 3, 3, 10);
            btnBrowse.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Anwendungen (*.exe)|*.exe|Alle Dateien (*.*)|*.*";
                    if (dialog.ShowDialog() == DialogResult.OK)
                        txtAppPath.Text = dialog.FileName;
                }
            };

            // Label Argumente
            var lblArgs = UIStyles.Labels.CreateNormal("Argumente:");
            lblArgs.Dock = DockStyle.Fill;
            lblArgs.Margin = new Padding(3, 5, 3, 3);

            // TextBox Argumente
            txtAppArguments = UIStyles.TextBoxes.CreateStandard();
            txtAppArguments.Name = "txtAppArguments";
            txtAppArguments.Dock = DockStyle.Fill;
            txtAppArguments.Margin = new Padding(3, 3, 3, 10);
            txtAppArguments.Multiline = false;

            // Horizontales Panel für Label + ToggleSwitch
            var horizontalPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            horizontalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            horizontalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            horizontalPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblOptions = UIStyles.Labels.CreateNormal("Als Admin ausführen:");
            lblOptions.Dock = DockStyle.Fill;
            lblOptions.Margin = new Padding(3, 5, 3, 3);

            chkRunAsAdmin = UIStyles.ToggleSwitches.CreateStandard(false, "Ja", "Nein");
            chkRunAsAdmin.Name = "chkRunAsAdmin";
            chkRunAsAdmin.Anchor = AnchorStyles.Left;
            chkRunAsAdmin.Margin = new Padding(3, 3, 3, 3);

            horizontalPanel.Controls.Add(lblOptions, 0, 0);
            horizontalPanel.Controls.Add(chkRunAsAdmin, 1, 0);

            // Controls hinzufügen
            stepTypeContentTlp.Controls.Add(lblPath, 0, 0);
            stepTypeContentTlp.SetColumnSpan(lblPath, 2);

            stepTypeContentTlp.Controls.Add(txtAppPath, 0, 1);
            stepTypeContentTlp.Controls.Add(btnBrowse, 1, 1);

            stepTypeContentTlp.Controls.Add(lblArgs, 0, 2);
            stepTypeContentTlp.SetColumnSpan(lblArgs, 2);

            stepTypeContentTlp.Controls.Add(txtAppArguments, 0, 3);
            stepTypeContentTlp.SetColumnSpan(txtAppArguments, 2);

            stepTypeContentTlp.Controls.Add(horizontalPanel, 0, 4);
            stepTypeContentTlp.SetColumnSpan(horizontalPanel, 2);

            stepTypeContentTlp.Visible = true;
        }

        // ========== BACK BUTTON ==========
        private void BtnBack_Click(object sender, EventArgs e)
        {
            CloseEditor();

            if (!ValidateRoutineName()) return;

            if (HasChanges())
            {
                SaveCurrentRoutine();
                SaveChanges?.Invoke(this, _currentRoutine);
            }

            BackToRoutinesClicked?.Invoke(this, EventArgs.Empty);
        }
        private bool ValidateRoutineName()
        {
            string routineName = txtRoutineName.Text.Trim();

            // Prüfen ob Name leer ist
            if (string.IsNullOrWhiteSpace(routineName))
            {
                if (!AutoConfirmDialogs)
                    MessageBox.Show("Bitte geben Sie einen Namen für die Routine ein.", "Validierung",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoutineName.Focus();
                return false;
            }

            // Prüfen ob Name bereits existiert
            var existingRoutines = _routineService.GetAllRoutines();
            bool nameExists = existingRoutines.Any(r =>
                r.Name.Equals(routineName, StringComparison.OrdinalIgnoreCase) &&
                r.Id != _currentRoutine.Id);

            if (nameExists)
            {
                if (!AutoConfirmDialogs)
                    MessageBox.Show($"Eine Routine mit dem Namen '{routineName}' existiert bereits.\nBitte wählen Sie einen anderen Namen.",
                        "Validierung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRoutineName.Focus();
                txtRoutineName.SelectAll();
                return false;
            }

            return true;
        }
        private bool ValidateCurrentStep(bool showMessageBox = true)
        {
            if (string.IsNullOrWhiteSpace(txtStepName.Text))
            {
                if (showMessageBox && !AutoConfirmDialogs)
                    MessageBox.Show("Bitte geben Sie einen Namen für den Schritt ein.", "Validierung",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStepName.Focus();
                return false;
            }

            if (cmbStepType.SelectedIndex == -1)
            {
                if (showMessageBox && !AutoConfirmDialogs)
                    MessageBox.Show("Bitte wählen Sie einen Aktionstyp aus.", "Validierung",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbStepType.Focus();
                return false;
            }

            var selectedType = (StepType)cmbStepType.SelectedValue;

            switch (selectedType)
            {
                case StepType.OpenUrl:
                    if (string.IsNullOrWhiteSpace(txtUrl?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                            MessageBox.Show("Bitte geben Sie eine URL ein.", "Validierung",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtUrl?.Focus();
                        return false;
                    }
                    break;
                case StepType.OpenFolder:
                    if (string.IsNullOrWhiteSpace(txtFolderPath?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                            MessageBox.Show("Bitte geben Sie einen Ordnerpfad ein.", "Validierung",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtFolderPath?.Focus();
                        return false;
                    }
                    break;
                case StepType.OpenApplication:
                    if (string.IsNullOrWhiteSpace(txtAppPath?.Text))
                    {
                        if (showMessageBox && !AutoConfirmDialogs)
                            MessageBox.Show("Bitte geben Sie einen Programmpfad ein.", "Validierung",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtAppPath?.Focus();
                        return false;
                    }
                    break;
            }

            return true;
        }
        private void SaveCurrentStep(bool refreshList = true)
        {
            Debug.WriteLine("=== SaveCurrentStep wurde aufgerufen ===");

            // Validierung ohne MessageBox (weil schon gezeigt)
            if (!ValidateCurrentStep(false)) return;

            string name = txtStepName.Text.Trim();
            string description = txtStepDescription.Text.Trim();
            bool show = tglStepEnabled.Checked;

            if (cmbStepType.SelectedIndex == -1) return;

            var selectedType = (StepType)cmbStepType.SelectedValue;
            RoutineStep step;

            switch (selectedType)
            {
                case StepType.OpenUrl:
                    step = new OpenUrlStep
                    {
                        Url = txtUrl?.Text ?? "",
                        OpenInExternBrowser = chkOpenInExternBrowser?.Checked ?? true
                    };
                    break;
                case StepType.OpenFolder:
                    step = new OpenFolderStep
                    {
                        FolderPath = txtFolderPath?.Text ?? "",
                        OpenInNewWindow = chkOpenInNewWindow?.Checked ?? true
                    };
                    break;
                case StepType.OpenApplication:
                    step = new OpenApplicationStep
                    {
                        ApplicationPath = txtAppPath?.Text ?? "",
                        Arguments = txtAppArguments?.Text ?? "",
                        RunAsAdmin = chkRunAsAdmin?.Checked ?? false
                    };
                    break;
                default:
                    throw new NotSupportedException();
            }

            step.Name = name;
            step.Description = description;
            step.Show = show;

            if (_editingStep != null)
            {
                step.Id = _editingStep.Id;
                step.Order = _editingStep.Order;
                _routineService.UpdateStep(_currentRoutine.Id, step);
                _editingStep = step;
                _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);
            }
            else
            {
                _routineService.AddStep(_currentRoutine.Id, step);
                _currentRoutine = _routineService.GetRoutine(_currentRoutine.Id);

                if (_originalRoutine != null)
                    _currentRoutine.Order = _originalRoutine.Order;

                _editingStep = _currentRoutine.Steps.LastOrDefault();
            }

            if (refreshList)
            {
                RefreshStepsList(silent: false);
            }
        }
        private void SaveCurrentRoutine()
        {
            _currentRoutine.Name = txtRoutineName.Text.Trim();
            _currentRoutine.UpdatedAt = DateTime.Now;
            _routineService.UpdateRoutine(_currentRoutine);
        }
    }
}
using SmartRoutine.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Forms
{
    public enum CustomMessageBoxButtons
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum CustomMessageBoxIcon
    {
        None,
        Info,
        Warning,
        Error,
        Question,
        Success
    }

    public static class CustomMessageBox
    {
        public static DialogResult Show(
            string message,
            string title = "Hinweis",
            CustomMessageBoxButtons buttons = CustomMessageBoxButtons.OK,
            CustomMessageBoxIcon icon = CustomMessageBoxIcon.Info,
            Form owner = null)
        {
            using (var form = new CustomMessageBoxForm(message, title, buttons, icon))
            {
                return owner != null
                    ? form.ShowDialog(owner)
                    : form.ShowDialog();
            }
        }
    }

    public partial class CustomMessageBoxForm : SmartRoutineForm
    {
        private readonly string _message;
        private readonly CustomMessageBoxButtons _buttons;

        public CustomMessageBoxForm(
            string message,
            string title,
            CustomMessageBoxButtons buttons,
            CustomMessageBoxIcon icon)
            : base(
                icon: GetTitleBarIcon(icon),
                title: title,
                showMinimize: false,
                showMaximize: false,
                showClose: true,
                allowWindowSnapAndMaximize: false)
        {
            _message = message;
            _buttons = buttons;

            ConfigureForm();
            BuildLayout();
        }

        private void ConfigureForm()
        {
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            Size = new Size(500, 220);
            MinimumSize = Size;
            MaximumSize = Size;
        }

        private void BuildLayout()
        {
            var rootPanel = UIStyles.Panels.CreateMedium();

            var mainTlp = UIStyles.TableLayoutPanels.CreateStandard(1, 2);
            mainTlp.Dock = DockStyle.Fill;
            mainTlp.BackColor = UIStyles.Colors.BackgroundMedium;
            mainTlp.Padding = new Padding(0);
            mainTlp.Margin = new Padding(0);

            mainTlp.RowStyles.Clear();
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

            mainTlp.Controls.Add(CreateContentPanel(), 0, 0);
            mainTlp.Controls.Add(CreateButtonPanel(), 0, 1);

            rootPanel.Controls.Add(mainTlp);

            ContentPanel.Controls.Clear();
            ContentPanel.Controls.Add(rootPanel);
        }
        private Control CreateContentPanel()
        {
            var panel = UIStyles.Panels.CreateMedium();
            panel.Padding = new Padding(28, 20, 28, 10);

            var messageLabel = UIStyles.Labels.CreateNormal(_message);
            messageLabel.Dock = DockStyle.Fill;
            messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            messageLabel.ForeColor = UIStyles.Colors.TextPrimary;
            messageLabel.Font = UIStyles.Fonts.Normal;
            messageLabel.BackColor = Color.Transparent;
            messageLabel.AutoEllipsis = false;

            panel.Controls.Add(messageLabel);

            return panel;
        }

        private Control CreateButtonPanel()
        {
            var buttonPanel = UIStyles.TableLayoutPanels.CreateStandard(1, 1);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.BackColor = UIStyles.Colors.BackgroundMedium;
            buttonPanel.Padding = new Padding(12, 12, 28, 18);
            buttonPanel.Margin = new Padding(0);

            var buttonInfos = GetButtons();

            buttonPanel.ColumnCount = buttonInfos.Length + 1;
            buttonPanel.ColumnStyles.Clear();
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            buttonPanel.RowStyles.Clear();
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            for (int i = 0; i < buttonInfos.Length; i++)
            {
                buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));

                var button = CreateDialogButton(buttonInfos[i].Text, buttonInfos[i].Result);
                buttonPanel.Controls.Add(button, i + 1, 0);
            }

            return buttonPanel;
        }
        private Button CreateDialogButton(string text, DialogResult result)
        {
            Button button;

            if (result == DialogResult.OK || result == DialogResult.Yes)
                button = UIStyles.Buttons.CreateGreen(text, "", new Size(120, 35));
            else if (result == DialogResult.No)
                button = UIStyles.Buttons.CreateDanger(text, "", new Size(120, 35));
            else
                button = UIStyles.Buttons.CreateStandard(text, "", new Size(120, 35));

            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(6, 0, 0, 0);
            button.DialogResult = result;

            if (result == DialogResult.OK || result == DialogResult.Yes)
                AcceptButton = button;

            if (result == DialogResult.Cancel || result == DialogResult.No)
                CancelButton = button;

            button.Click += (s, e) =>
            {
                DialogResult = result;
                Close();
            };

            return button;
        }

        private (string Text, DialogResult Result)[] GetButtons()
        {
            switch (_buttons)
            {
                case CustomMessageBoxButtons.OK:
                    return new[]
                    {
                        ("✓", DialogResult.OK)
                    };

                case CustomMessageBoxButtons.OKCancel:
                    return new[]
                    {
                        ("✓", DialogResult.OK),
                        ("Abbrechen", DialogResult.Cancel)
                    };

                case CustomMessageBoxButtons.YesNo:
                    return new[]
                    {
                        ("✓", DialogResult.Yes),
                        ("✖", DialogResult.No)
                    };

                case CustomMessageBoxButtons.YesNoCancel:
                    return new[]
                    {
                        ("✓", DialogResult.Yes),
                        ("✖", DialogResult.No),
                        ("Abbrechen", DialogResult.Cancel)
                    };

                default:
                    return new[]
                    {
                        ("✓", DialogResult.OK)
                    };
            }
        }
        private static Image GetTitleBarIcon(CustomMessageBoxIcon icon)
        {
            switch (icon)
            {
                case CustomMessageBoxIcon.Info:
                    return SystemIcons.Information.ToBitmap();

                case CustomMessageBoxIcon.Warning:
                    return SystemIcons.Warning.ToBitmap();

                case CustomMessageBoxIcon.Error:
                    return SystemIcons.Error.ToBitmap();

                case CustomMessageBoxIcon.Question:
                    return SystemIcons.Question.ToBitmap();

                case CustomMessageBoxIcon.Success:
                    return SystemIcons.Shield.ToBitmap();

                default:
                    return null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.None)
            {
                switch (_buttons)
                {
                    case CustomMessageBoxButtons.OKCancel:
                    case CustomMessageBoxButtons.YesNoCancel:
                        DialogResult = DialogResult.Cancel;
                        break;

                    case CustomMessageBoxButtons.YesNo:
                        DialogResult = DialogResult.No;
                        break;

                    default:
                        DialogResult = DialogResult.OK;
                        break;
                }
            }

            base.OnFormClosing(e);
        }
    }
}

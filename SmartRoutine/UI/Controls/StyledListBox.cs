using SmartRoutine.Data.Models;
using SmartRoutine.UI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SmartRoutine.UI.Controls
{
    public class StyledListBox : ListBox
    {
        private Color _itemBackColor = UIStyles.Colors.BackgroundMedium;
        private Color _itemForeColor = UIStyles.Colors.TextPrimary;
        private Color _selectedBackColor = UIStyles.Colors.Primary;
        private Color _selectedForeColor = UIStyles.Colors.White;
        private Color _hoverBackColor = UIStyles.Colors.BackgroundLight;
        private Color _dragHandleColor = UIStyles.Colors.TextTertiary;
        private Color _dragIndicatorColor = UIStyles.Colors.PrimaryLight;
        private int _itemHeight = 35;
        private const int DRAG_HANDLE_WIDTH = 30;           // Gesamtbreite für Hit-Test
        private const int DRAG_HANDLE_DRAW_WIDTH = 22;      // Tatsächliche Breite der gezeichneten Linien
        private const int DRAG_HANDLE_HIT_AREA_PADDING = 5;  // Zusätzlicher Bereich links/rechts

        // Drag & Drop
        private int _dragIndex = -1;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private int _dragInsertPosition = -1;

        public event EventHandler ItemsReordered;
        public Func<object, string> DisplayTextProvider { get; set; }

        public string DisplayTextMember { get; set; }

        public bool ShowEnumeration { get; set; } = false;

        private bool _allowReorder = true;

        public bool AllowReorder
        {
            get => _allowReorder;
            set
            {
                _allowReorder = value;
                AllowDrop = value;
                Invalidate();
            }
        }
        private Color _disabledForeColor = UIStyles.Colors.TextDisabled;
        private Color _disabledBackColor = UIStyles.Colors.BackgroundDarkElevated;
        public Func<object, bool> IsItemDisabled { get; set; }

        public Color DisabledForeColor
        {
            get => _disabledForeColor;
            set { _disabledForeColor = value; Invalidate(); }
        }

        public Color DisabledBackColor
        {
            get => _disabledBackColor;
            set { _disabledBackColor = value; Invalidate(); }
        }

        public StyledListBox(bool allowReorder = false)
        {
            _allowReorder = allowReorder;

            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.ItemHeight = _itemHeight;
            this.BackColor = UIStyles.Colors.BackgroundDark;
            this.ForeColor = _itemForeColor;
            this.BorderStyle = BorderStyle.None;
            this.Font = new Font("Segoe UI", 9.5f);
            this.IntegralHeight = false;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            this.AllowDrop = _allowReorder;
        }

        private string GetDisplayText(object item, int index)
        {
            string text;

            if (DisplayTextProvider != null)
            {
                text = DisplayTextProvider(item);
            }
            else if (!string.IsNullOrWhiteSpace(DisplayTextMember))
            {
                var property = item?.GetType().GetProperty(DisplayTextMember);
                text = property?.GetValue(item)?.ToString() ?? "";
            }
            else
            {
                text = item?.ToString() ?? "";
            }

            return ShowEnumeration
                ? $"{index + 1}. {text}"
                : text;
        }
        public int ItemHeightCustom
        {
            get => _itemHeight;
            set { _itemHeight = value; this.ItemHeight = value; Invalidate(); }
        }

        public Color DragIndicatorColor
        {
            get => _dragIndicatorColor;
            set { _dragIndicatorColor = value; Invalidate(); }
        }

        private int _hoverIndex = -1;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int index = IndexFromPoint(e.Location);

            if (!_isDragging)
            {
                if (_hoverIndex != index)
                {
                    int oldHoverIndex = _hoverIndex;
                    _hoverIndex = index;

                    InvalidateItem(oldHoverIndex);
                    InvalidateItem(_hoverIndex);
                }
            }

            if (_allowReorder && e.Button == MouseButtons.Left && !_isDragging && _dragIndex != -1)
            {
                if (Math.Abs(e.X - _dragStartPoint.X) > SystemInformation.DragSize.Width ||
                    Math.Abs(e.Y - _dragStartPoint.Y) > SystemInformation.DragSize.Height)
                {
                    _isDragging = true;
                    DoDragDrop(_dragIndex, DragDropEffects.Move);
                }
            }

            base.OnMouseMove(e);
        }

        private void InvalidateItem(int index)
        {
            if (index < 0 || index >= Items.Count)
                return;

            Invalidate(GetItemRectangle(index));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int index = IndexFromPoint(e.Location);

            if (_allowReorder && index != -1)
            {
                Rectangle itemRect = GetItemRectangle(index);
                Rectangle dragRect = GetDragHandleRectangle(itemRect, IsVerticalScrollBarVisible());

                if (dragRect.Contains(e.Location))
                {
                    _dragIndex = index;
                    _dragStartPoint = e.Location;
                    SelectedIndex = index;
                    return;
                }
            }

            if (index == -1)
            {
                ClearSelected();
                _dragIndex = -1;
                return;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isDragging = false;
            _dragIndex = -1;
            _dragInsertPosition = -1;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            if (!_allowReorder)
            {
                drgevent.Effect = DragDropEffects.None;
                return;
            }
            Point point = PointToClient(new Point(drgevent.X, drgevent.Y));
            int targetIndex = IndexFromPoint(point);
            int newInsertPosition = -1;

            if (Items.Count == 0)
            {
                newInsertPosition = 0;
                drgevent.Effect = DragDropEffects.Move;
            }
            else if (targetIndex != -1)
            {
                Rectangle itemRect = GetItemRectangle(targetIndex);
                bool isTopHalf = point.Y < itemRect.Y + itemRect.Height / 2;

                if (isTopHalf)
                {
                    newInsertPosition = targetIndex;
                }
                else
                {
                    newInsertPosition = targetIndex + 1;
                }
                drgevent.Effect = DragDropEffects.Move;
            }
            else if (Items.Count > 0 && point.Y > GetItemRectangle(Items.Count - 1).Bottom)
            {
                newInsertPosition = Items.Count;
                drgevent.Effect = DragDropEffects.Move;
            }
            else
            {
                drgevent.Effect = DragDropEffects.None;
            }

            if (_dragInsertPosition != newInsertPosition)
            {
                _dragInsertPosition = newInsertPosition;
                Invalidate();
            }
            
            // Automatisches Scrollen während Drag & Drop
            if (point.Y < 20)
            {
                int topIndex = TopIndex;
                if (topIndex > 0)
                {
                    TopIndex = topIndex - 1;
                    drgevent.Effect = DragDropEffects.Move;
                }
            }
            else if (point.Y > ClientSize.Height - 20)
            {
                int topIndex = TopIndex;
                if (topIndex < Items.Count - 1)
                {
                    TopIndex = topIndex + 1;
                    drgevent.Effect = DragDropEffects.Move;
                }
            }

            base.OnDragOver(drgevent);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (!_allowReorder)
            {
                _isDragging = false;
                _dragIndex = -1;
                _dragInsertPosition = -1;
                return;
            }
            if (_dragIndex != -1 && _dragInsertPosition != -1)
            {
                int targetPos = _dragInsertPosition;
                if (targetPos > _dragIndex)
                {
                    targetPos--;
                }

                targetPos = Math.Max(0, Math.Min(targetPos, Items.Count));

                if (targetPos != _dragIndex && targetPos >= 0 && targetPos <= Items.Count)
                {
                    BeginUpdate();
                    try
                    {
                        object draggedItem = Items[_dragIndex];
                        Items.RemoveAt(_dragIndex);
                        if (targetPos > Items.Count) targetPos = Items.Count;
                        Items.Insert(targetPos, draggedItem);
                        SelectedIndex = targetPos;

                        if (targetPos < TopIndex)
                        {
                            TopIndex = targetPos;
                        }
                        else if (targetPos > TopIndex + (ClientSize.Height / ItemHeight) - 1)
                        {
                            TopIndex = targetPos - (ClientSize.Height / ItemHeight) + 1;
                        }

                        ItemsReordered?.Invoke(this, EventArgs.Empty);
                    }
                    finally
                    {
                        EndUpdate();
                    }
                }
            }

            _isDragging = false;
            _dragIndex = -1;
            _dragInsertPosition = -1;
            _hoverIndex = -1;
            Invalidate();

            base.OnDragDrop(drgevent);
        }
        // Hilfsmethode: Gibt den Rechteck-Bereich des Drag-Handles für ein bestimmtes Item zurück
        private Rectangle GetDragHandleRectangle(Rectangle itemRect, bool isVScrollVisible)
        {
            int x = itemRect.Right - DRAG_HANDLE_WIDTH;  // Verwende DRAG_HANDLE_WIDTH als Basis

            if (isVScrollVisible)
            {
                x = itemRect.Right - DRAG_HANDLE_WIDTH - SystemInformation.VerticalScrollBarWidth;
            }

            // Hit-Bereich mit Padding
            return new Rectangle(
                x - DRAG_HANDLE_HIT_AREA_PADDING,
                itemRect.Y,
                DRAG_HANDLE_WIDTH + (DRAG_HANDLE_HIT_AREA_PADDING * 2),
                itemRect.Height
            );
        }


        // Hilfsmethode: Prüft ob Scrollleiste sichtbar ist
        private bool IsVerticalScrollBarVisible()
        {
            if (Items.Count == 0) return false;
            return (Items.Count * ItemHeight) > ClientSize.Height;
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            gfbevent.UseDefaultCursors = false;
            Cursor.Current = Cursors.SizeAll;
            base.OnGiveFeedback(gfbevent);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_isDragging)
            {
                int oldHoverIndex = _hoverIndex;
                _hoverIndex = -1;
                InvalidateItem(oldHoverIndex);
            }
            else
            {
                _dragInsertPosition = -1;
                Invalidate();
            }

            base.OnMouseLeave(e);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

            object item = Items[e.Index];
            string itemText = GetDisplayText(item, e.Index);
            bool isDisabled = IsItemDisabled?.Invoke(item) == true;

            Color backColor;

            if ((e.State & DrawItemState.Selected) != 0 && !_isDragging)
            {
                backColor = _selectedBackColor;
            }
            else if (e.Index == _hoverIndex && !_isDragging)
            {
                backColor = _hoverBackColor;
            }
            else if (isDisabled)
            {
                backColor = _disabledBackColor;
            }
            else
            {
                backColor = e.Index % 2 == 0
                    ? _itemBackColor
                    : Color.FromArgb(_itemBackColor.R - 5, _itemBackColor.G - 5, _itemBackColor.B - 5);
            }

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            Color textColor;

            if ((e.State & DrawItemState.Selected) != 0 && !_isDragging)
            {
                textColor = _selectedForeColor;
            }
            else if (isDisabled)
            {
                textColor = _disabledForeColor;
            }
            else
            {
                textColor = _itemForeColor;
            }

            bool isVScrollVisible = IsVerticalScrollBarVisible();

            Rectangle dragRect = Rectangle.Empty;

            if (_allowReorder)
            {
                dragRect = GetDragHandleRectangle(rect, isVScrollVisible);
                DrawDragHandle(e.Graphics, dragRect);
            }

            int textRightMargin = 12;
            int textLeftMargin = 8;
            int reservedDragWidth = _allowReorder ? dragRect.Width : 0;

            Rectangle textRect = new Rectangle(
                rect.X + textLeftMargin,
                rect.Y,
                rect.Width - reservedDragWidth - textLeftMargin - textRightMargin - (isVScrollVisible ? SystemInformation.VerticalScrollBarWidth : 0),
                rect.Height
            );

            using (var textBrush = new SolidBrush(textColor))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                e.Graphics.DrawString(itemText, this.Font, textBrush, textRect, format);
            }

            base.OnDrawItem(e);
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // Nach dem Zeichnen aller Items die Linie zeichnen
            if (m.Msg == 0x0F) // WM_PAINT
            {
                DrawDragIndicator();
            }
        }

        private void DrawDragIndicator()
        {
            if (!_isDragging || _dragInsertPosition == -1) return;

            using (Graphics g = this.CreateGraphics())
            {
                int yPos;
                if (_dragInsertPosition == 0)
                {
                    yPos = 0;
                }
                else if (_dragInsertPosition >= Items.Count)
                {
                    if (Items.Count > 0)
                    {
                        var lastRect = GetItemRectangle(Items.Count - 1);
                        yPos = lastRect.Bottom;
                    }
                    else
                    {
                        yPos = 0;
                    }
                }
                else
                {
                    var rect = GetItemRectangle(_dragInsertPosition);
                    yPos = rect.Top;
                }

                yPos = Math.Max(0, yPos);

                using (var pen = new Pen(_dragIndicatorColor, 3))
                {
                    g.DrawLine(pen, 0, yPos, this.Width, yPos);
                }
            }
        }

        private void DrawDragHandle(Graphics g, Rectangle dragRect)
        {
            using (var pen = new Pen(_dragHandleColor, 1.5f))
            {
                int centerY = dragRect.Y + dragRect.Height / 2;

                // Zentriere die Balken im Hit-Bereich (nicht im sichtbaren Bereich)
                int centerX = dragRect.X + (dragRect.Width / 2);
                int startX = centerX - 6;  // Balkenbreite 12px
                int lineHeight = 3;
                int spacing = 1;

                for (int i = 0; i < 3; i++)
                {
                    int y = centerY - lineHeight - spacing + (i * (lineHeight + spacing));
                    g.DrawLine(pen, startX, y, startX + 12, y);
                }
            }
        }

        public void ClearSelected()
        {
            if (SelectedIndex != -1)
            {
                SelectedIndex = -1;
                Invalidate();
            }
        }

        public void MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Items.Count || toIndex < 0 || toIndex >= Items.Count)
                return;

            BeginUpdate();
            try
            {
                object item = Items[fromIndex];
                Items.RemoveAt(fromIndex);
                Items.Insert(toIndex, item);
                SelectedIndex = toIndex;
                ItemsReordered?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                EndUpdate();
            }
        }
    }
}
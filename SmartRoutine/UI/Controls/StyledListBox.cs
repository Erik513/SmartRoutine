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
        private const int DRAG_HANDLE_WIDTH = 30;

        // Drag & Drop
        private int _dragIndex = -1;
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private int _dragInsertPosition = -1;

        public event EventHandler ItemsReordered;

        public StyledListBox()
        {
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

            this.AllowDrop = true;
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
                    _hoverIndex = index;
                    Invalidate();
                }
            }

            if (e.Button == MouseButtons.Left && !_isDragging && _dragIndex != -1)
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int index = IndexFromPoint(e.Location);

            if (index != -1 && e.X > this.Width - DRAG_HANDLE_WIDTH)
            {
                _dragIndex = index;
                _dragStartPoint = e.Location;
                this.SelectedIndex = index;
                return;
            }

            if (index == -1)
            {
                this.ClearSelected();
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

            base.OnDragOver(drgevent);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (_dragIndex != -1 && _dragInsertPosition != -1)
            {
                int targetPos = _dragInsertPosition;
                if (targetPos > _dragIndex)
                {
                    targetPos--;
                }

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
                _hoverIndex = -1;
                Invalidate();
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

            Color backColor;
            if ((e.State & DrawItemState.Selected) != 0 && !_isDragging)
            {
                backColor = _selectedBackColor;
            }
            else if (e.Index == _hoverIndex && !_isDragging)
            {
                backColor = _hoverBackColor;
            }
            else
            {
                backColor = e.Index % 2 == 0 ? _itemBackColor : Color.FromArgb(_itemBackColor.R - 5, _itemBackColor.G - 5, _itemBackColor.B - 5);
            }

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            Color textColor = ((e.State & DrawItemState.Selected) != 0 && !_isDragging) ? _selectedForeColor : _itemForeColor;

            // Drag-Handle
            Rectangle dragRect = new Rectangle(rect.Right - DRAG_HANDLE_WIDTH, rect.Y, DRAG_HANDLE_WIDTH - 8, rect.Height);
            DrawDragHandle(e.Graphics, dragRect);

            // Text
            object item = Items[e.Index];
            string itemText = item?.ToString() ?? "";

            using (var textBrush = new SolidBrush(textColor))
            {
                var textRect = new Rectangle(rect.X + 8, rect.Y, rect.Width - DRAG_HANDLE_WIDTH - 12, rect.Height);
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

                using (var pen = new Pen(_dragIndicatorColor, 3))
                {
                    g.DrawLine(pen, 0, yPos, this.Width, yPos);
                }
            }
        }

        private void DrawDragHandle(Graphics g, Rectangle rect)
        {
            using (var pen = new Pen(_dragHandleColor, 1.5f))
            {
                int centerY = rect.Y + rect.Height / 2;
                int x = rect.X + 5;
                int lineHeight = 3;
                int spacing = 1;

                for (int i = 0; i < 3; i++)
                {
                    int y = centerY - lineHeight - spacing + (i * (lineHeight + spacing));
                    g.DrawLine(pen, x, y, x + 12, y);
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
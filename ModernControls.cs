using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace xeno_rat_server
{
    internal static class Shape
    {
        public static GraphicsPath Rounded(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0 || r.Width <= 0 || r.Height <= 0)
            {
                path.AddRectangle(r);
                return path;
            }
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class RoundedButton : Button
    {
        public int CornerRadius { get; set; } = 12;
        public int ShadowDepth { get; set; } = 6;
        public Color HoverColor { get; set; } = Color.Empty;

        private bool _hover;
        private bool _pressed;

        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(124, 111, 232);
            ForeColor = Color.White;
            Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            Cursor = Cursors.Hand;
            Size = new Size(150, 48);
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.FromArgb(37, 37, 58));

            int pad = Math.Max(2, ShadowDepth);
            var face = new Rectangle(pad, pad, Width - pad * 2 - 1, Height - pad * 2 - 1);

            if (Enabled)
            {
                for (int i = pad; i >= 1; i--)
                {
                    var sr = new Rectangle(face.X - i, face.Y - i + 3, face.Width + 2 * i, face.Height + 2 * i);
                    int a = (int)(38 * (1.0 - (double)(i - 1) / pad));
                    using (var sp = Shape.Rounded(sr, CornerRadius + i))
                    using (var sb = new SolidBrush(Color.FromArgb(Math.Max(0, a), 0, 0, 0)))
                        g.FillPath(sb, sp);
                }
            }

            Color fill = !Enabled ? Color.FromArgb(70, 70, 90)
                       : _pressed ? ControlPaint.Dark(BackColor, 0.05f)
                       : _hover ? (HoverColor.IsEmpty ? ControlPaint.Light(BackColor, 0.18f) : HoverColor)
                       : BackColor;

            using (var path = Shape.Rounded(face, CornerRadius))
            using (var b = new SolidBrush(fill))
                g.FillPath(b, path);

            TextRenderer.DrawText(g, Text, Font, face, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    public class DarkTabControl : TabControl
    {
        public Color StripColor { get; set; } = Color.FromArgb(24, 24, 37);
        public Color PageColor { get; set; } = Color.FromArgb(37, 37, 58);
        public Color TabColor { get; set; } = Color.FromArgb(45, 45, 68);
        public Color SelectedColor { get; set; } = Color.FromArgb(124, 111, 232);
        public Color TextColor { get; set; } = Color.FromArgb(150, 150, 180);
        public Color SelectedTextColor { get; set; } = Color.White;

        public DarkTabControl()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(180, 44);
            Font = new Font("Segoe UI Semibold", 10.5F);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(StripColor);

            using (var b = new SolidBrush(PageColor))
                g.FillRectangle(b, Rectangle.Inflate(DisplayRectangle, 3, 3));

            for (int i = 0; i < TabCount; i++)
            {
                Rectangle rect = GetTabRect(i);
                Rectangle fill = Rectangle.Inflate(rect, -3, -4);
                bool selected = i == SelectedIndex;

                Color back = selected ? SelectedColor : TabColor;
                Color fore = selected ? SelectedTextColor : TextColor;

                using (var path = Shape.Rounded(fill, 10))
                using (var sb = new SolidBrush(back))
                    g.FillPath(sb, path);

                TextRenderer.DrawText(g, TabPages[i].Text, Font, fill, fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }
    }

    public class DarkListView : ListView
    {
        public Color RowColor { get; set; } = Color.FromArgb(37, 37, 58);
        public Color AltRowColor { get; set; } = Color.FromArgb(42, 42, 64);
        public Color HeaderColor { get; set; } = Color.FromArgb(24, 24, 37);
        public Color SelectionColor { get; set; } = Color.FromArgb(124, 111, 232);
        public Color TextColor { get; set; } = Color.FromArgb(224, 224, 234);
        public Color HeaderTextColor { get; set; } = Color.FromArgb(160, 160, 185);
        private Color LineColor = Color.FromArgb(55, 55, 80);

        public DarkListView()
        {
            OwnerDraw = true;
            View = View.Details;
            FullRowSelect = true;
            BorderStyle = BorderStyle.None;
            BackColor = Color.FromArgb(37, 37, 58);
            ForeColor = Color.FromArgb(224, 224, 234);
            Font = new Font("Segoe UI", 10F);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0014)
            {
                using (var g = Graphics.FromHdc(m.WParam))
                using (var brush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }
                m.Result = (IntPtr)1;
            }
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            using (var b = new SolidBrush(HeaderColor))
                e.Graphics.FillRectangle(b, e.Bounds);

            using (var p = new Pen(LineColor, 1))
            {
                e.Graphics.DrawLine(p, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
                e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            var headerFont = new Font("Segoe UI Semibold", 9.5F);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, headerFont, e.Bounds, HeaderTextColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.LeftAndRightPadding);
            headerFont.Dispose();
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            bool selected = e.Item.Selected;
            Color back = selected ? SelectionColor : (e.ItemIndex % 2 == 0 ? RowColor : AltRowColor);
            Color fore = selected ? Color.White : TextColor;

            using (var b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, e.Bounds);

            using (var p = new Pen(LineColor, 1))
            {
                e.Graphics.DrawLine(p, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);

                if (e.ColumnIndex == this.Columns.Count - 1)
                {
                    int remainingWidth = this.ClientSize.Width - e.Bounds.Right;
                    if (remainingWidth > 0)
                    {
                        using (var b2 = new SolidBrush(back))
                            e.Graphics.FillRectangle(b2, e.Bounds.Right, e.Bounds.Top, remainingWidth, e.Bounds.Height);
                    }
                }

                if (e.ColumnIndex == 0)
                {
                    e.Graphics.DrawLine(p, 0, e.Bounds.Bottom - 1, this.Width, e.Bounds.Bottom - 1);
                }
            }

            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Font, e.Bounds, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.LeftAndRightPadding | TextFormatFlags.EndEllipsis);
        }
    }
}
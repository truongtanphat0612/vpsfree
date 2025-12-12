using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ToolCheckInfo.App.UI.Helpers
{
    public static class UIThemeHelper
    {
        private static readonly Color AppBackColor = Color.FromArgb(30, 30, 30);
        private static readonly Color TextColor = Color.White;
        private static readonly Color InputBackColor = Color.FromArgb(50, 50, 50);
        private static readonly Color GridBackColor = Color.FromArgb(45, 45, 48);
        private static readonly Color GroupBoxHeaderColor = ColorTranslator.FromHtml("#E67E22");

        public static void ApplyDarkTheme(Form form, DataGridView dgv)
        {
            form.BackColor = AppBackColor;
            form.ForeColor = TextColor;
            form.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            if (dgv != null)
            {
                SetDoubleBuffered(dgv);
                dgv.BackgroundColor = GridBackColor;
                dgv.GridColor = Color.FromArgb(60, 60, 60);
                dgv.BorderStyle = BorderStyle.None;
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                dgv.DefaultCellStyle.BackColor = AppBackColor;
                dgv.DefaultCellStyle.ForeColor = TextColor;
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
                dgv.DefaultCellStyle.SelectionForeColor = TextColor;
                dgv.RowHeadersVisible = false;
            }

            foreach (Control c in form.Controls) ApplyColorToControl(c);
        }

        private static void ApplyColorToControl(Control c)
        {
            if (c is GroupBox gb)
            {
                gb.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                gb.FlatStyle = FlatStyle.Flat;
                gb.ForeColor = Color.White;
                gb.Paint -= GroupBox_Paint;
                gb.Paint += GroupBox_Paint;
                foreach (Control child in gb.Controls) ApplyColorToControl(child);
            }
            else if (c is TextBox || c is NumericUpDown || c is ComboBox)
            {
                c.BackColor = InputBackColor;
                c.ForeColor = TextColor;
            }
            else if (c is Label || c is CheckBox)
            {
                c.ForeColor = TextColor;
                c.BackColor = AppBackColor;
                c.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            }
            else if (c is Button btn)
            {
                StyleButton(btn, btn.BackColor == SystemColors.Control ? Color.FromArgb(60, 60, 60) : btn.BackColor);
            }
        }

        private static void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 7, box.Width - 1, box.Height - 8);
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1))
            {
                g.DrawRectangle(pen, rect);
            }

            SizeF textSize = g.MeasureString(box.Text, box.Font);
            Rectangle titleRect = new Rectangle(6, 0, (int)textSize.Width + 6, (int)textSize.Height + 2);
            using (SolidBrush backBrush = new SolidBrush(AppBackColor)) g.FillRectangle(backBrush, titleRect);
            using (SolidBrush textBrush = new SolidBrush(GroupBoxHeaderColor)) g.DrawString(box.Text, box.Font, textBrush, 8, 0);
        }

        public static void StyleButton(Button btn, Color color)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            btn.MouseEnter += (s, e) => { btn.BackColor = ControlPaint.Light(color, 0.2f); };
            btn.MouseLeave += (s, e) => { btn.BackColor = color; };
        }

        private static void SetDoubleBuffered(Control c)
        {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession) return;
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }
    }
}
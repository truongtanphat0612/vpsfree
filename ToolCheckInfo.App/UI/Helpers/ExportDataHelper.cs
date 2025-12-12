using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ToolCheckInfo.Core.Models;

namespace ToolCheckInfo.App.UI.Helpers
{
    public class ExportDataHelper
    {
        public static void ShowExportDialog(List<Facebook> accounts)
        {
            Form exportForm = new Form();
            exportForm.Text = "Tùy chọn Xuất Dữ Liệu";
            exportForm.Size = new System.Drawing.Size(350, 400);
            exportForm.StartPosition = FormStartPosition.CenterParent;
            exportForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            exportForm.MaximizeBox = false;
            exportForm.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            exportForm.ForeColor = System.Drawing.Color.White;

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Top;
            panel.Height = 300;
            panel.FlowDirection = FlowDirection.TopDown;
            panel.Padding = new Padding(20);
            panel.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);

            var columns = new Dictionary<string, string>
            {
                { "Uid", "UID" },
                { "Password", "Password" },
                { "Cookie", "Cookie" },
                { "Token", "Token" },
                { "Status", "Trạng Thái" },
                { "Country", "Quốc Gia" }
            };

            List<CheckBox> checkBoxes = new List<CheckBox>();
            foreach (var col in columns)
            {
                CheckBox chk = new CheckBox();
                chk.Text = col.Value;
                chk.Tag = col.Key;
                chk.Checked = true;
                chk.AutoSize = true;
                chk.ForeColor = System.Drawing.Color.White;
                panel.Controls.Add(chk);
                checkBoxes.Add(chk);
            }

            Button btnExport = new Button();
            btnExport.Text = "XUẤT FILE";
            btnExport.DialogResult = DialogResult.OK;
            btnExport.Size = new System.Drawing.Size(120, 40);
            btnExport.Location = new System.Drawing.Point(110, 310);
            btnExport.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnExport.ForeColor = System.Drawing.Color.White;
            btnExport.FlatStyle = FlatStyle.Flat;

            exportForm.Controls.Add(panel);
            exportForm.Controls.Add(btnExport);

            if (exportForm.ShowDialog() == DialogResult.OK)
            {
                PerformExport(accounts, checkBoxes);
            }
        }

        private static void PerformExport(List<Facebook> accounts, List<CheckBox> checkboxes)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File|*.txt|CSV File|*.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                var selectedProps = checkboxes.Where(c => c.Checked).Select(c => c.Tag.ToString()).ToList();
                var headers = checkboxes.Where(c => c.Checked).Select(c => c.Text).ToList();

                if (sfd.FileName.EndsWith(".csv")) sb.AppendLine(string.Join(",", headers));

                foreach (var acc in accounts)
                {
                    List<string> values = new List<string>();
                    foreach (var prop in selectedProps)
                    {
                        var val = acc.GetType().GetProperty(prop)?.GetValue(acc, null)?.ToString() ?? "";
                        values.Add(val);
                    }
                    string separator = sfd.FileName.EndsWith(".csv") ? "," : "|";
                    sb.AppendLine(string.Join(separator, values));
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Xuất file thành công!", "Thông báo");
            }
        }
    }
}
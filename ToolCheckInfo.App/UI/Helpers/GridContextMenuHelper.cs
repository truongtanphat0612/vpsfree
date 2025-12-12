using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ToolCheckInfo.Core.Models;
using ToolCheckInfo.Infrastructure.Selenium;

namespace ToolCheckInfo.App.UI.Helpers
{
    public class GridContextMenuHelper
    {
        private DataGridView _dataGridView;
        private ContextMenuStrip _ctxMenu;

        public GridContextMenuHelper(DataGridView dgv)
        {
            _dataGridView = dgv;
            InitContextMenu();
        }

        private void InitContextMenu()
        {
            _ctxMenu = new ContextMenuStrip();

            ToolStripMenuItem itemCopyUid = new ToolStripMenuItem("Copy UID");
            itemCopyUid.Click += (s, e) => CopyCellData(2);

            ToolStripMenuItem itemCopyPass = new ToolStripMenuItem("Copy Password");
            itemCopyPass.Click += (s, e) => CopyCellData(3);

            ToolStripMenuItem itemCopyCookie = new ToolStripMenuItem("Copy Cookie");
            itemCopyCookie.Click += (s, e) => CopyCellData(4);

            ToolStripMenuItem itemCopyLine = new ToolStripMenuItem("Copy Dòng (Full)");
            itemCopyLine.Click += (s, e) => CopySelectedLines();

            // Nút Mở Trình Duyệt Tự Động Đăng Nhập
            ToolStripMenuItem itemOpenBrowser = new ToolStripMenuItem("Mở Trình Duyệt (Auto Login)");
            itemOpenBrowser.Click += (s, e) => OpenSelectedBrowser();

            ToolStripMenuItem itemExport = new ToolStripMenuItem("Xuất Excel (Cũ)");
            itemExport.Click += (s, e) => ExportToCsv();

            // Nút Xuất Tùy Chỉnh
            ToolStripMenuItem itemCustomExport = new ToolStripMenuItem("Xuất Dữ Liệu (Tùy Chỉnh)");
            itemCustomExport.Click += (s, e) => {
                List<Facebook> listToExport = new List<Facebook>();
                if (_dataGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng bôi đen ít nhất 1 dòng!", "Thông báo");
                    return;
                }
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                {
                    var f = new Facebook();
                    f.Uid = row.Cells[2].Value?.ToString();
                    f.Password = row.Cells[3].Value?.ToString();
                    f.Cookie = row.Cells[4].Value?.ToString();
                    f.Folder = row.Cells[5].Value?.ToString();
                    f.Status = row.Cells[6].Value?.ToString();
                    listToExport.Add(f);
                }
                ExportDataHelper.ShowExportDialog(listToExport);
            };

            _ctxMenu.Items.AddRange(new ToolStripItem[] {
                itemCopyUid, itemCopyPass, itemCopyCookie, new ToolStripSeparator(),
                itemCopyLine, new ToolStripSeparator(),
                itemOpenBrowser, new ToolStripSeparator(),
                itemExport, itemCustomExport
            });

            _dataGridView.ContextMenuStrip = _ctxMenu;
        }

        private void CopyCellData(int cellIndex)
        {
            if (_dataGridView.SelectedRows.Count > 0)
            {
                string data = "";
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                {
                    if (cellIndex < row.Cells.Count && row.Cells[cellIndex].Value != null)
                        data += row.Cells[cellIndex].Value.ToString() + "\r\n";
                }
                if (!string.IsNullOrEmpty(data)) Clipboard.SetText(data);
            }
        }

        private void CopySelectedLines()
        {
            if (_dataGridView.SelectedRows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (DataGridViewRow row in _dataGridView.SelectedRows)
                {
                    List<string> cells = new List<string>();
                    for (int i = 1; i < _dataGridView.Columns.Count; i++)
                    {
                        if (_dataGridView.Columns[i].Visible)
                            cells.Add(row.Cells[i].Value?.ToString() ?? "");
                    }
                    sb.AppendLine(string.Join("|", cells));
                }
                if (sb.Length > 0) Clipboard.SetText(sb.ToString());
            }
        }

        private void OpenSelectedBrowser()
        {
            if (_dataGridView.SelectedRows.Count == 1)
            {
                var uid = _dataGridView.SelectedRows[0].Cells[2].Value?.ToString();
                var pass = _dataGridView.SelectedRows[0].Cells[3].Value?.ToString();
                var cookie = _dataGridView.SelectedRows[0].Cells[4].Value?.ToString();

                if (!string.IsNullOrEmpty(uid))
                {
                    Facebook fbAccount = new Facebook { Uid = uid, Password = pass, Cookie = cookie };

                    Thread thread = new Thread(() => {
                        try
                        {
                            // Mở Chrome không ẩn để người dùng xem
                            Chrome chrome = new Chrome(uid, 100, 100);
                            chrome.OpenProfile();

                            if (chrome.driver != null)
                            {
                                LoginPage login = new LoginPage(chrome, fbAccount);

                                bool cookieOk = false;
                                if (!string.IsNullOrEmpty(cookie))
                                {
                                    login.Login_Cookie(cookie);
                                    Thread.Sleep(2000);
                                    if (login.Check_Login() == "OK") cookieOk = true;
                                }

                                if (!cookieOk && !string.IsNullOrEmpty(pass))
                                {
                                    login.Login_With_Pass();
                                }

                                chrome.GoTo("https://www.facebook.com/");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Lỗi mở trình duyệt: " + ex.Message);
                        }
                    });
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn duy nhất 1 dòng để mở!", "Thông báo");
            }
        }

        private void ExportToCsv()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV File|*.csv";
            sfd.FileName = "Result_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                List<string> headers = new List<string>();
                for (int i = 1; i < _dataGridView.Columns.Count; i++)
                {
                    if (_dataGridView.Columns[i].Visible) headers.Add(_dataGridView.Columns[i].HeaderText);
                }
                sb.AppendLine(string.Join(",", headers));

                foreach (DataGridViewRow row in _dataGridView.Rows)
                {
                    List<string> cells = new List<string>();
                    for (int i = 1; i < _dataGridView.Columns.Count; i++)
                    {
                        if (_dataGridView.Columns[i].Visible)
                        {
                            string val = row.Cells[i].Value?.ToString() ?? "";
                            if (val.Contains(",")) val = "\"" + val + "\"";
                            cells.Add(val);
                        }
                    }
                    sb.AppendLine(string.Join(",", cells));
                }
                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Xuất file thành công!", "Thông báo");
            }
        }
    }
}
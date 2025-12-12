using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolCheckInfo.Core.Models;

namespace ToolCheckInfo.App.UI.Helpers
{
    public class GridFacebook
    {
        public static string SELECT = "SELECT";
        public static string STT = "STT";
        public static string UID = "UID";
        public static string PASSWORD = "PASSWORD";
        public static string COOKIE = "COOKIE";
        public static string FOLDER = "FOLDER";
        public static string STATUS = "STATUS";
        public static string STTOFFB = "STTOFFB";
        public static string STATUS_HIDE = "STATUS_HIDE";

        private static DataGridView GridViewFacebook;
        private static Dictionary<int, DataGridViewRow> _rowMap = new Dictionary<int, DataGridViewRow>();

        public static void SetGridFacebook(DataGridView dataGridView)
        {
            GridViewFacebook = dataGridView;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.AllowUserToAddRows = false;

            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewCheckBoxColumn checkColumn = new DataGridViewCheckBoxColumn();
            checkColumn.Name = SELECT;
            checkColumn.HeaderText = "Chọn";
            checkColumn.Width = 50;
            checkColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView.Columns.Add(checkColumn);

            dataGridView.Columns.Add(STT, "Stt");
            dataGridView.Columns.Add(UID, "Uid");
            dataGridView.Columns.Add(PASSWORD, "Pass");
            dataGridView.Columns.Add(COOKIE, "Cookie");
            dataGridView.Columns.Add(FOLDER, "Folder");
            dataGridView.Columns.Add(STATUS, "Status");
            dataGridView.Columns.Add(STTOFFB, "Stt_Hide");
            dataGridView.Columns.Add(STATUS_HIDE, "Status_Hide");

            dataGridView.Columns[STT].FillWeight = 5f;
            dataGridView.Columns[STT].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView.Columns[STT].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView.Columns[UID].FillWeight = 15f;
            dataGridView.Columns[UID].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView.Columns[PASSWORD].FillWeight = 10f;
            dataGridView.Columns[PASSWORD].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView.Columns[COOKIE].FillWeight = 15f;
            dataGridView.Columns[COOKIE].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView.Columns[FOLDER].FillWeight = 20f;
            dataGridView.Columns[FOLDER].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView.Columns[STATUS].FillWeight = 30f;
            dataGridView.Columns[STATUS].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView.Columns[STTOFFB].Visible = false;
            dataGridView.Columns[STATUS_HIDE].Visible = false;
        }

        public static void LoadData(List<Facebook> facebooks)
        {
            if (GridViewFacebook == null) return;
            GridViewFacebook.Rows.Clear();
            _rowMap.Clear();

            for (int i = 0; i < facebooks.Count; i++)
            {
                Facebook f = facebooks[i];
                try
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(GridViewFacebook);
                    row.Cells[0].Value = false;
                    row.Cells[1].Value = i + 1;
                    row.Cells[2].Value = f.Uid;
                    row.Cells[3].Value = f.Password;
                    row.Cells[4].Value = f.Cookie;
                    row.Cells[5].Value = Path.GetFileName(f.Folder);
                    row.Cells[6].Value = f.Status;
                    row.Cells[7].Value = f.id;
                    row.Cells[8].Value = f.Status;
                    GridViewFacebook.Rows.Add(row);

                    if (!_rowMap.ContainsKey(f.id))
                    {
                        _rowMap.Add(f.id, GridViewFacebook.Rows[GridViewFacebook.Rows.Count - 1]);
                    }
                }
                catch { }
            }
        }

        public static bool IsUidChecked(int originalId)
        {
            if (GridViewFacebook == null) return false;

            if (_rowMap.TryGetValue(originalId, out DataGridViewRow row))
            {
                if (GridViewFacebook.InvokeRequired)
                {
                    bool result = false;
                    GridViewFacebook.Invoke((MethodInvoker)delegate {
                        result = Convert.ToBoolean(row.Cells[0].Value);
                    });
                    return result;
                }
                else
                {
                    return Convert.ToBoolean(row.Cells[0].Value);
                }
            }
            return false;
        }

        public static List<Facebook> GetSelectedAccounts(List<Facebook> sourceList)
        {
            List<Facebook> selected = new List<Facebook>();
            if (GridViewFacebook == null) return selected;

            foreach (DataGridViewRow row in GridViewFacebook.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value))
                {
                    if (row.Cells[7].Value != null && int.TryParse(row.Cells[7].Value.ToString(), out int id))
                    {
                        var acc = sourceList.FirstOrDefault(f => f.id == id);
                        if (acc != null) selected.Add(acc);
                    }
                }
            }
            return selected;
        }

        public static void Update_Status(Facebook acc, string column, string comment)
        {
            if (GridViewFacebook == null) return;

            if (_rowMap.TryGetValue(acc.id, out DataGridViewRow row))
            {
                int statusColIndex = GetIndexColumn(column);
                if (GridViewFacebook.InvokeRequired)
                {
                    GridViewFacebook.BeginInvoke((MethodInvoker)delegate {
                        try { row.Cells[statusColIndex].Value = comment; } catch { }
                    });
                }
                else
                {
                    try { row.Cells[statusColIndex].Value = comment; } catch { }
                }
            }
        }

        public static void Update_RowColor(Facebook acc, Color color)
        {
            if (GridViewFacebook == null) return;

            if (_rowMap.TryGetValue(acc.id, out DataGridViewRow row))
            {
                if (GridViewFacebook.InvokeRequired)
                {
                    GridViewFacebook.BeginInvoke((MethodInvoker)delegate {
                        try { row.DefaultCellStyle.BackColor = color; } catch { }
                    });
                }
                else
                {
                    try { row.DefaultCellStyle.BackColor = color; } catch { }
                }
            }
        }

        public static void DeleteSelectedRows(List<Facebook> dataSource)
        {
            if (GridViewFacebook == null) return;
            List<DataGridViewRow> rowsToDelete = new List<DataGridViewRow>();
            List<string> uidsToDelete = new List<string>();
            foreach (DataGridViewRow row in GridViewFacebook.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells[0].Value);
                if (isChecked || row.Selected)
                {
                    rowsToDelete.Add(row);
                    var uid = row.Cells[GetIndexColumn(UID)].Value?.ToString();
                    if (uid != null) uidsToDelete.Add(uid);
                }
            }
            dataSource.RemoveAll(x => uidsToDelete.Contains(x.Uid));
            foreach (var row in rowsToDelete) { if (!row.IsNewRow) GridViewFacebook.Rows.Remove(row); }
        }

        public static int GetIndexColumn(string columnName)
        {
            for (int i = 0; i < GridViewFacebook.Columns.Count; i++)
            {
                if (GridViewFacebook.Columns[i].Name == columnName) return i;
            }
            return -1;
        }
    }
}
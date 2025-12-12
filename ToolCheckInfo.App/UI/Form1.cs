using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolCheckInfo.App.Config;
using ToolCheckInfo.App.UI.Helpers;
using ToolCheckInfo.Core.Constants;
using ToolCheckInfo.Core.Models;
using ToolCheckInfo.Infrastructure.Helpers;
using ToolCheckInfo.Infrastructure.Network;
using ToolCheckInfo.Infrastructure.Selenium;
using ToolCheckInfo.Infrastructure.Services;
using ToolCheckInfo.Core.Interfaces;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using OpenQA.Selenium.Chrome;
using Group = ToolCheckInfo.Core.Models.Group;

namespace ToolCheckInfo.App.UI
{
    public class Form1 : Form
    {
        private List<Facebook> _allAccounts = new List<Facebook>();
        private List<Facebook> _displayedAccounts = new List<Facebook>();

        private SemaphoreSlim _semaphore;
        private bool _isStopScan = false;
        private bool _isRunning = false;
        private ManualResetEvent _pauseEvent = new ManualResetEvent(true);
        private static object _slotLock = new object();
        private List<int> _freeSlots;

        private System.ComponentModel.IContainer components = null;
        private Button btnReadFolder, btnScan, btnDelete, btnManageProxy, btnPause, btnUpdateDriver;
        private DataGridView dataGridViewFacebook;
        private TextBox txtPath, txtIdTele, txtBotTele, txtSearch;
        private ComboBox cboFilter;
        private GroupBox groupBox1, groupBox2, groupBox3;
        private Label label1, label2, label3, lblCopyright, lblStats;
        private NumericUpDown numberThread;
        private CheckBox chkHideChrome, chkProxy;
        private CheckBox chkScanAds, chkScanPage, chkScanGroup;
        private PictureBox picLogo;
        private ProgressBar prgImport;
        private System.Windows.Forms.Timer timerStats;
        private System.Windows.Forms.Timer timerAutoSave;
        private bool isAllSelected = false;
        private bool isPaused = false;
        private GridContextMenuHelper menuHelper;

        public Form1()
        {
            InitializeComponent();
            CheckAndSetupDriver();
            try { if (File.Exists("app.ico")) this.Icon = new Icon("app.ico"); } catch { }

            UIThemeHelper.StyleButton(btnReadFolder, Color.FromArgb(0, 122, 204));
            UIThemeHelper.StyleButton(btnScan, Color.FromArgb(40, 167, 69));
            UIThemeHelper.StyleButton(btnDelete, Color.FromArgb(220, 53, 69));
            UIThemeHelper.StyleButton(btnManageProxy, Color.FromArgb(108, 117, 125));
            UIThemeHelper.StyleButton(btnPause, Color.FromArgb(255, 193, 7));
            UIThemeHelper.StyleButton(btnUpdateDriver, Color.FromArgb(138, 43, 226));

            GridFacebook.SetGridFacebook(dataGridViewFacebook);
            SetupGridEvents();
            menuHelper = new GridContextMenuHelper(dataGridViewFacebook);

            ConfigUIHelper.LoadConfigToUI(numberThread, chkHideChrome, chkProxy, txtBotTele, txtIdTele);
            ConfigSetting.HIDECHROME = chkHideChrome.Checked;
            ConfigSetting.THREADS = (int)numberThread.Value;

            ItemHelper.btnScan = btnScan;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.FormClosing += Form1_FormClosing;

            if (this.components == null) this.components = new System.ComponentModel.Container();
            timerStats = new System.Windows.Forms.Timer(this.components) { Interval = 1000 };
            timerStats.Tick += (s, e) => { UpdateUIStats(); };
            timerStats.Start();

            timerAutoSave = new System.Windows.Forms.Timer(this.components) { Interval = 30000 };
            timerAutoSave.Tick += TimerAutoSave_Tick;
            timerAutoSave.Start();
        }

        #region Driver & Setup
        private void CheckAndSetupDriver()
        {
            Task.Run(() =>
            {
                string driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromedriver.exe");
                // Logic mới: Chỉ update nếu file chưa tồn tại
                if (!File.Exists(driverPath))
                {
                    try
                    {
                        new DriverManager().SetUpDriver(new ChromeConfig());
                    }
                    catch { }
                }
                // Nếu file đã tồn tại, bỏ qua kiểm tra phiên bản (để tiết kiệm thời gian)
            });
        }

        // ... (phần còn lại giữ nguyên)

        private void SetupGridEvents()
        {
            if (dataGridViewFacebook.Columns.Count > 0)
                dataGridViewFacebook.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewFacebook.ReadOnly = false;
            foreach (DataGridViewColumn col in dataGridViewFacebook.Columns) if (col.Index != 0) col.ReadOnly = true;
            dataGridViewFacebook.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dataGridViewFacebook.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewFacebook.AllowUserToResizeRows = false;
            dataGridViewFacebook.ColumnHeaderMouseClick += DataGridViewFacebook_ColumnHeaderMouseClick;
            dataGridViewFacebook.KeyDown += DataGridViewFacebook_KeyDown;
            dataGridViewFacebook.CellContentClick += DataGridViewFacebook_CellContentClick;
        }
        #endregion

        #region Event Handlers
        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshGrid();
            if (cboFilter.Items.Count == 0) cboFilter.Items.AddRange(new object[] { "All", "New", "Live", "Die", "Ads" });
            if (cboFilter.Items.Count > 0) cboFilter.SelectedIndex = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemHelper.KillChromeDrivers();
        }

        private void TimerAutoSave_Tick(object sender, EventArgs e) { }

        private void btnUpdateDriver_Click(object sender, EventArgs e)
        {
            if (_isRunning) { MessageBox.Show("Vui lòng dừng tool trước khi cập nhật!", "Cảnh báo"); return; }
            btnUpdateDriver.Text = "..."; btnUpdateDriver.Enabled = false;
            Task.Run(() =>
            {
                string driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromedriver.exe");
                // Update thủ công: Luôn xóa cũ tải mới
                if (File.Exists(driverPath)) { try { File.Delete(driverPath); } catch { } }

                try
                {
                    new DriverManager().SetUpDriver(new ChromeConfig());
                    this.Invoke((MethodInvoker)delegate { btnUpdateDriver.Text = "Upd Driver"; btnUpdateDriver.Enabled = true; MessageBox.Show("Đã cập nhật Driver!", "Thông báo"); });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate { btnUpdateDriver.Text = "Upd Driver"; btnUpdateDriver.Enabled = true; MessageBox.Show("Lỗi update: " + ex.Message, "Lỗi"); });
                }
            });
        }

        private async void btnReadFolder_Click(object sender, EventArgs e)
        {
            try
            {
                using FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtPath.Text = fbd.SelectedPath;
                    LoadingForm loading = new LoadingForm();
                    loading.Show();

                    await Task.Run(() =>
                    {
                        string[] files = Directory.GetFiles(fbd.SelectedPath, "*.txt", SearchOption.AllDirectories);
                        foreach(var file in files)
                        {
                            var lines = File.ReadAllLines(file);
                            foreach(var line in lines)
                            {
                                if(line.Contains("|"))
                                {
                                    var parts = line.Split('|');
                                    if(parts.Length >= 2)
                                    {
                                        var acc = new Facebook { Uid = parts[0].Trim(), Password = parts[1].Trim(), Cookie = line.Contains("c_user") ? line : "", ImportDate = DateTime.Now.ToString("dd/MM/yyyy") };
                                        if(!_allAccounts.Any(x => x.Uid == acc.Uid)) _allAccounts.Add(acc);
                                    }
                                }
                            }
                        }
                    });

                    if (!loading.IsDisposed) loading.Close();

                    _displayedAccounts = _allAccounts;
                    RefreshGrid();
                    MessageBox.Show($"Đã import xong! Tổng số: {_allAccounts.Count}", "Thành công");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if ((_isRunning && !_isStopScan) || _isStopScan)
            {
                _isStopScan = true;
                return;
            }

            List<Facebook> selectedAccounts = GridFacebook.GetSelectedAccounts(_allAccounts);
            if (selectedAccounts.Count == 0) { MessageBox.Show("Vui lòng chọn tài khoản để chạy!", "Cảnh báo"); return; }

            AppConfig.UserTelegramBotToken = txtBotTele.Text;
            AppConfig.UserTelegramChatId = txtIdTele.Text;

            ConfigSetting.HIDECHROME = chkHideChrome.Checked;
            ConfigSetting.THREADS = (int)numberThread.Value;

            _isRunning = true;
            _isStopScan = false;
            btnScan.Text = "Stop";
            btnScan.BackColor = Color.FromArgb(220, 53, 69);
            ProxyManger.SetProxy(ConfigSetting.PROXYS);

            Task.Run(() => StartScanProcess(selectedAccounts));
        }

        private void StartScanProcess(List<Facebook> accounts)
        {
            _semaphore = new SemaphoreSlim(ConfigSetting.THREADS);
            InitWindowSlots(ConfigSetting.THREADS + 5);

            List<Task> runningTasks = new List<Task>();
            foreach (var account in accounts)
            {
                if (_isStopScan) break;
                _pauseEvent.WaitOne();
                _semaphore.Wait();

                int slot = GetFreeSlot();
                Task task = Task.Run(async () =>
                {
                    try { await ProcessAccount(account, slot); }
                    catch (Exception ex) { FileLog.AddDataToFile(FileLog.ConvertErrorToString(ex)); }
                    finally { ReleaseSlot(slot); _semaphore.Release(); }
                });
                runningTasks.Add(task);
            }
            Task.WaitAll(runningTasks.ToArray());

            this.Invoke((MethodInvoker)delegate
            {
                btnScan.Text = "Start";
                btnScan.BackColor = Color.FromArgb(40, 167, 69);
                _isRunning = false;
                MessageBox.Show("Hoàn thành quét!", "Thông báo");
            });
        }

        private async Task ProcessAccount(Facebook account, int slotIndex)
        {
            if (!GridFacebook.IsUidChecked(account.id)) return;

            string proxy = "";
            if (chkProxy.Checked && ConfigSetting.PROXYS.Count > 0) proxy = ProxyManger.GetNextProxy();

            int width = 400; int height = 650;
            int screenW = Screen.PrimaryScreen.WorkingArea.Width;
            int cols = screenW / width; if (cols < 1) cols = 1;
            int r = slotIndex / cols; int c = slotIndex % cols;
            Chrome chrome = new Chrome("", c * width, r * height, $"{width},{height}", "", proxy);

            try
            {
                UpdateStatus(account, "Đang khởi tạo Chrome...", null);
                chrome.OpenProfile();

                if (chrome.driver == null)
                {
                    UpdateStatus(account, "Lỗi Chrome Driver", null);
                    return;
                }

                // --- LOGIN PHASE ---
                UpdateStatus(account, "Bắt đầu đăng nhập...", null);
                ILoginService loginService = new LoginService(chrome);

                string loginStatus = await loginService.LoginAsync(account, (msg, color) => UpdateStatus(account, msg, color));

                if (loginStatus == "OK")
                {
                    GridFacebook.Update_RowColor(account, Color.LightGreen);
                    UpdateStatus(account, "Login OK. Đang quét...", null);

                    account.Cookie = chrome.GetCookie();
                    account.Token = loginService.GetType().GetMethod("GetToken")?.Invoke(loginService, null) as string;

                    // --- SCAN PHASE ---
                    RestShapeClient client = new RestShapeClient(account.Cookie);
                    IScanService scanService = new ScanService(chrome, client);

                    InforAccount info = await scanService.GetAccountInfoAsync(account, (msg, color) => UpdateStatus(account, msg, color));

                    // --- REPORT PHASE ---
                    IReportService reportService = new ReportService();
                    await reportService.SendReportAsync(info, account, AppDomain.CurrentDomain.BaseDirectory);

                    UpdateStatus(account, "Hoàn thành!", null);
                    account.Status = "DONE";
                }
                else
                {
                    GridFacebook.Update_RowColor(account, Color.OrangeRed);
                    UpdateStatus(account, loginStatus, null);
                    account.Status = loginStatus;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus(account, "Lỗi ngoại lệ: " + ex.Message, null);
            }
            finally
            {
                if (chrome != null) chrome.Exit();
            }
        }

        private void UpdateStatus(Facebook account, string message, Color? color)
        {
            GridFacebook.Update_Status(account, GridFacebook.STATUS, message);
            if(color.HasValue) GridFacebook.Update_RowColor(account, color.Value);
        }

        private void InitWindowSlots(int maxThreads) { lock (_slotLock) { _freeSlots = new List<int>(); for (int i = 0; i < maxThreads; i++) _freeSlots.Add(i); } }
        private int GetFreeSlot() { lock (_slotLock) { if (_freeSlots.Count > 0) { int slot = _freeSlots[0]; _freeSlots.RemoveAt(0); return slot; } return 0; } }
        private void ReleaseSlot(int slot) { lock (_slotLock) { if (!_freeSlots.Contains(slot)) { _freeSlots.Add(slot); _freeSlots.Sort(); } } }

        private void btnDelete_Click(object sender, EventArgs e) { /* Delete logic */ }
        private void btnPause_Click(object sender, EventArgs e) {
             if (!_isRunning) return;
             isPaused = !isPaused;
             if (isPaused) { _pauseEvent.Reset(); btnPause.Text = "Resume"; btnPause.BackColor = Color.OrangeRed; }
             else { _pauseEvent.Set(); btnPause.Text = "Pause"; btnPause.BackColor = Color.FromArgb(255, 193, 7); }
        }

        private void RefreshGrid() {
            var list = _allAccounts;
            if(!string.IsNullOrEmpty(txtSearch.Text)) list = list.Where(x => x.Uid.Contains(txtSearch.Text)).ToList();
            GridFacebook.LoadData(list);
        }

        private void UpdateUIStats()
        {
            int selected = 0;
            foreach (DataGridViewRow row in dataGridViewFacebook.Rows) if (Convert.ToBoolean(row.Cells[0].Value)) selected++;
            if(lblStats != null) lblStats.Text = $"Selected: {selected} | Total: {_allAccounts.Count}";
        }

        private void SaveConfig() { ConfigUIHelper.SaveConfigFromUI(numberThread.Value, chkHideChrome.Checked, chkProxy.Checked, txtBotTele.Text, txtIdTele.Text); }
        private void CboFilter_SelectedIndexChanged(object sender, EventArgs e) => RefreshGrid();
        private void TxtSearch_TextChanged(object sender, EventArgs e) => RefreshGrid();
        private void numberThread_ValueChanged(object sender, EventArgs e) => SaveConfig();
        private void chkHideChrome_CheckedChanged(object sender, EventArgs e) => SaveConfig();
        private void chkScanAds_CheckedChanged(object sender, EventArgs e) { ConfigSetting.SCAN_ADS = chkScanAds.Checked; }
        private void chkScanPage_CheckedChanged(object sender, EventArgs e) { ConfigSetting.SCAN_PAGE = chkScanPage.Checked; }
        private void chkScanGroup_CheckedChanged(object sender, EventArgs e) { ConfigSetting.SCAN_GROUP = chkScanGroup.Checked; }
        private void txtBotTele_TextChanged(object sender, EventArgs e) => SaveConfig();
        private void txtIdTele_TextChanged(object sender, EventArgs e) => SaveConfig();
        private void chkProxy_CheckedChanged(object sender, EventArgs e) => SaveConfig();
        private void btnManageProxy_Click(object sender, EventArgs e) { FormProxy frm = new FormProxy(ConfigSetting.PROXYS); frm.ShowDialog(); }
        private void DataGridViewFacebook_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) { if (e.ColumnIndex == 0) { isAllSelected = !isAllSelected; dataGridViewFacebook.SuspendLayout(); foreach (DataGridViewRow row in dataGridViewFacebook.Rows) row.Cells[0].Value = isAllSelected; dataGridViewFacebook.ResumeLayout(); dataGridViewFacebook.EndEdit(); } }
        private void DataGridViewFacebook_CellContentClick(object sender, DataGridViewCellEventArgs e) { if (e.ColumnIndex == 0 && e.RowIndex >= 0) dataGridViewFacebook.CommitEdit(DataGridViewDataErrorContexts.Commit); }
        private void DataGridViewFacebook_KeyDown(object sender, KeyEventArgs e) { /* ... */ }
        private void Form1_KeyDown(object sender, KeyEventArgs e) { /* ... */ }

        #region UI Initialization (Generated Code Compacted)
        private void InitializeComponent()
        {
            this.btnReadFolder = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnManageProxy = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnUpdateDriver = new System.Windows.Forms.Button();
            this.prgImport = new System.Windows.Forms.ProgressBar();
            this.dataGridViewFacebook = new System.Windows.Forms.DataGridView();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.txtIdTele = new System.Windows.Forms.TextBox();
            this.txtBotTele = new System.Windows.Forms.TextBox();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.cboFilter = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblStats = new System.Windows.Forms.Label();
            this.numberThread = new System.Windows.Forms.NumericUpDown();
            this.chkHideChrome = new System.Windows.Forms.CheckBox();
            this.chkProxy = new System.Windows.Forms.CheckBox();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.chkScanAds = new System.Windows.Forms.CheckBox();
            this.chkScanPage = new System.Windows.Forms.CheckBox();
            this.chkScanGroup = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)this.dataGridViewFacebook).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.numberThread).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.picLogo).BeginInit();
            base.SuspendLayout();
            this.groupBox2.Location = new System.Drawing.Point(12, 10);
            this.groupBox2.Size = new System.Drawing.Size(260, 100);
            this.groupBox2.Text = "Control";
            this.groupBox2.Controls.Add(this.prgImport);
            this.groupBox2.Controls.Add(this.btnPause);
            this.groupBox2.Controls.Add(this.btnDelete);
            this.groupBox2.Controls.Add(this.btnReadFolder);
            this.groupBox2.Controls.Add(this.txtPath);
            this.groupBox2.Controls.Add(this.btnScan);
            this.groupBox2.Controls.Add(this.btnUpdateDriver);
            this.btnReadFolder.Location = new System.Drawing.Point(6, 22); this.btnReadFolder.Size = new System.Drawing.Size(90, 23); this.btnReadFolder.Text = "Open File"; this.btnReadFolder.UseVisualStyleBackColor = true; this.btnReadFolder.Click += new System.EventHandler(this.btnReadFolder_Click);
            this.txtPath.Location = new System.Drawing.Point(100, 22); this.txtPath.Size = new System.Drawing.Size(150, 23);
            this.prgImport.Location = new System.Drawing.Point(6, 47); this.prgImport.Size = new System.Drawing.Size(244, 5); this.prgImport.Visible = false;
            this.btnScan.Location = new System.Drawing.Point(6, 60); this.btnScan.Size = new System.Drawing.Size(58, 25); this.btnScan.Text = "Start"; this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            this.btnPause.Location = new System.Drawing.Point(70, 60); this.btnPause.Size = new System.Drawing.Size(58, 25); this.btnPause.Text = "Pause"; this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            this.btnDelete.Location = new System.Drawing.Point(134, 60); this.btnDelete.Size = new System.Drawing.Size(58, 25); this.btnDelete.Text = "Del"; this.btnDelete.BackColor = Color.Red; this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            this.btnUpdateDriver.Location = new System.Drawing.Point(198, 60); this.btnUpdateDriver.Size = new System.Drawing.Size(56, 25); this.btnUpdateDriver.Text = "Upd"; this.btnUpdateDriver.Click += new System.EventHandler(this.btnUpdateDriver_Click);
            this.groupBox1.Location = new System.Drawing.Point(280, 10);
            this.groupBox1.Size = new System.Drawing.Size(320, 100);
            this.groupBox1.Text = "Configuration";
            this.groupBox1.Controls.Add(this.btnManageProxy); this.groupBox1.Controls.Add(this.chkProxy); this.groupBox1.Controls.Add(this.chkHideChrome); this.groupBox1.Controls.Add(this.numberThread); this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chkScanAds); this.groupBox1.Controls.Add(this.chkScanPage); this.groupBox1.Controls.Add(this.chkScanGroup);
            this.groupBox3.Location = new System.Drawing.Point(610, 10);
            this.groupBox3.Size = new System.Drawing.Size(300, 100);
            this.groupBox3.Text = "Telegram Notification";
            this.groupBox3.Controls.Add(this.label3); this.groupBox3.Controls.Add(this.txtIdTele); this.groupBox3.Controls.Add(this.label2); this.groupBox3.Controls.Add(this.txtBotTele);
            this.label2.Location = new System.Drawing.Point(10, 25); this.label2.Text = "Token:"; this.label2.AutoSize = true;
            this.txtBotTele.Location = new System.Drawing.Point(80, 22); this.txtBotTele.Size = new System.Drawing.Size(200, 23); this.txtBotTele.TextChanged += new System.EventHandler(this.txtBotTele_TextChanged);
            this.label3.Location = new System.Drawing.Point(10, 55); this.label3.Text = "ChatID:"; this.label3.AutoSize = true;
            this.txtIdTele.Location = new System.Drawing.Point(80, 52); this.txtIdTele.Size = new System.Drawing.Size(200, 23); this.txtIdTele.TextChanged += new System.EventHandler(this.txtIdTele_TextChanged);
            this.txtSearch.Location = new System.Drawing.Point(920, 50);
            this.txtSearch.Size = new System.Drawing.Size(250, 23);
            this.txtSearch.PlaceholderText = "Search / Tìm kiếm...";
            this.txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            this.cboFilter.Location = new System.Drawing.Point(1180, 50);
            this.cboFilter.Size = new System.Drawing.Size(100, 23); this.cboFilter.SelectedIndexChanged += new System.EventHandler(this.CboFilter_SelectedIndexChanged);
            this.dataGridViewFacebook.Location = new System.Drawing.Point(12, 120); this.dataGridViewFacebook.Size = new System.Drawing.Size(1600, 520); this.dataGridViewFacebook.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Text = "Copyright © 2025 Developed by d3f4ult";
            this.lblCopyright.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic);
            this.lblCopyright.ForeColor = Color.Black;
            this.lblCopyright.Location = new Point(1350, 650);
            this.lblCopyright.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.picLogo.Size = new System.Drawing.Size(32, 32);
            this.picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            this.picLogo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.picLogo.Location = new Point(1310, 642);
            if (File.Exists("logo.png")) try { this.picLogo.Image = Image.FromFile("logo.png"); } catch { }
            this.lblStats.Location = new System.Drawing.Point(12, 655);
            this.lblStats.AutoSize = true;
            this.lblStats.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.lblStats.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblStats.ForeColor = Color.Crimson;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1630, 678);
            this.Controls.Add(this.cboFilter); this.Controls.Add(this.lblStats); this.Controls.Add(this.txtSearch); this.Controls.Add(this.lblCopyright); this.Controls.Add(this.picLogo);
            this.Controls.Add(this.groupBox3); this.Controls.Add(this.groupBox2); this.Controls.Add(this.groupBox1); this.Controls.Add(this.dataGridViewFacebook);
            this.Text = "TOOL SCAN FACEBOOK v1.0 - Premium Edition by d3f4ult";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)this.dataGridViewFacebook).EndInit();
            this.groupBox1.ResumeLayout(false); this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false); this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false); this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.numberThread).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.picLogo).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }
    }
}

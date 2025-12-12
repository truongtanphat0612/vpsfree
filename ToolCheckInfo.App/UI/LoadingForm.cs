using System;
using System.Drawing;
using System.Windows.Forms;

namespace ToolCheckInfo
{
    public class LoadingForm : Form
    {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblCountInfo;
        private Button btnStop;
        private Button btnPause;

        public bool IsStopped { get; private set; } = false;
        public bool IsPaused { get; private set; } = false;

        public LoadingForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.progressBar = new ProgressBar();
            this.lblStatus = new Label();
            this.lblCountInfo = new Label();
            this.btnStop = new Button();
            this.btnPause = new Button();
            this.SuspendLayout();

            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new Point(12, 10);
            this.lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.lblStatus.ForeColor = Color.DimGray;
            this.lblStatus.Text = "Đang quét dữ liệu...";

            this.lblCountInfo.AutoSize = true;
            this.lblCountInfo.Location = new Point(200, 10);
            this.lblCountInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblCountInfo.ForeColor = Color.Teal;
            this.lblCountInfo.Text = "Tìm thấy: 0";
            this.lblCountInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            this.progressBar.Location = new Point(15, 35);
            this.progressBar.Size = new Size(355, 25);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;

            this.btnPause.Location = new Point(60, 75);
            this.btnPause.Size = new Size(100, 32);
            this.btnPause.Text = "Pause";
            this.btnPause.BackColor = Color.DarkOrange;
            this.btnPause.ForeColor = Color.White;
            this.btnPause.FlatStyle = FlatStyle.Flat;
            this.btnPause.FlatAppearance.BorderSize = 0;
            this.btnPause.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnPause.Click += BtnPause_Click;

            this.btnStop.Location = new Point(190, 75);
            this.btnStop.Size = new Size(130, 32);
            this.btnStop.Text = "Stop (Lấy Data)";
            this.btnStop.BackColor = Color.Firebrick;
            this.btnStop.ForeColor = Color.White;
            this.btnStop.FlatStyle = FlatStyle.Flat;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnStop.Click += BtnStop_Click;

            this.ClientSize = new Size(390, 125);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblCountInfo);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.WhiteSmoke;
            this.Text = "Tiến trình quét thư mục";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            IsPaused = !IsPaused;
            if (IsPaused)
            {
                btnPause.Text = "Continue";
                btnPause.BackColor = Color.ForestGreen;
                lblStatus.Text = "Đang tạm dừng...";
                progressBar.Style = ProgressBarStyle.Blocks;
            }
            else
            {
                btnPause.Text = "Pause";
                btnPause.BackColor = Color.DarkOrange;
                lblStatus.Text = "Đang tiếp tục...";
                progressBar.Style = ProgressBarStyle.Marquee;
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            IsStopped = true;
            this.Close();
        }

        public void UpdateCount(int count, string status = "")
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateCount(count, status)));
                return;
            }

            if (IsStopped) return;

            lblCountInfo.Text = $"Tìm thấy: {count}";
            lblCountInfo.Location = new Point(progressBar.Right - lblCountInfo.Width, lblCountInfo.Location.Y);

            if (!string.IsNullOrEmpty(status) && !IsPaused)
            {
                lblStatus.Text = status;
            }
        }
    }
}
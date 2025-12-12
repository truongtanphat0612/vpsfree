using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ToolCheckInfo.App.UI;

public class KeyForm : Form
{
    public static bool IsActive;

    // Khai báo các biến control
    private IContainer components = null;
    private Label label1;
    private TextBox txtKey;
    private Button btnOke;
    private Label lblMesssage;
    private LinkLabel linkLabel1;

    public KeyForm(string key, string status)
    {
        // Gọi hàm khởi tạo giao diện
        InitializeComponent();

        // Gán giá trị để biến được sử dụng, tránh cảnh báo
        if (lblMesssage != null) lblMesssage.Text = status;
        if (txtKey != null) txtKey.Text = key;
    }

    private void btnOke_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    // Hàm này khởi tạo các đối tượng giao diện
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.label1 = new System.Windows.Forms.Label();
        this.txtKey = new System.Windows.Forms.TextBox();
        this.btnOke = new System.Windows.Forms.Button();
        this.lblMesssage = new System.Windows.Forms.Label();
        this.linkLabel1 = new System.Windows.Forms.LinkLabel();
        this.SuspendLayout();

        // label1
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(12, 50);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(38, 15);
        this.label1.TabIndex = 0;
        this.label1.Text = "Key:";

        // txtKey
        this.txtKey.Location = new System.Drawing.Point(60, 47);
        this.txtKey.Name = "txtKey";
        this.txtKey.Size = new System.Drawing.Size(260, 23);
        this.txtKey.TabIndex = 1;

        // btnOke
        this.btnOke.Location = new System.Drawing.Point(120, 90);
        this.btnOke.Name = "btnOke";
        this.btnOke.Size = new System.Drawing.Size(100, 30);
        this.btnOke.TabIndex = 2;
        this.btnOke.Text = "OK";
        this.btnOke.UseVisualStyleBackColor = true;
        this.btnOke.Click += new System.EventHandler(this.btnOke_Click);

        // lblMesssage
        this.lblMesssage.AutoSize = true;
        this.lblMesssage.Location = new System.Drawing.Point(12, 20);
        this.lblMesssage.Name = "lblMesssage";
        this.lblMesssage.Size = new System.Drawing.Size(0, 15);
        this.lblMesssage.TabIndex = 3;

        // linkLabel1
        this.linkLabel1.AutoSize = true;
        this.linkLabel1.Location = new System.Drawing.Point(12, 130);
        this.linkLabel1.Name = "linkLabel1";
        this.linkLabel1.Size = new System.Drawing.Size(60, 15);
        this.linkLabel1.TabIndex = 4;
        this.linkLabel1.Text = "Contact";

        // KeyForm
        this.ClientSize = new System.Drawing.Size(350, 160);
        this.Controls.Add(this.linkLabel1);
        this.Controls.Add(this.lblMesssage);
        this.Controls.Add(this.btnOke);
        this.Controls.Add(this.txtKey);
        this.Controls.Add(this.label1);
        this.Name = "KeyForm";
        this.Text = "Activator";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
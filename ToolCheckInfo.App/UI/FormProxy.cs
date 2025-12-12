using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ToolCheckInfo.ConstanData;
using ToolCheckInfo.Core.Models;
using ToolCheckInfo.Infrastructure.Helpers;

namespace ToolCheckInfo;

public class FormProxy : Form
{
	private IContainer components = null;

	private TextBox txtProxy;

	public FormProxy(List<string> proxies)
	{
		InitializeComponent();
		if (proxies == null || proxies.Count == 0)
		{
			return;
		}
		foreach (string proxy in proxies)
		{
			txtProxy.Text = txtProxy.Text + proxy + "\r\n";
		}
	}

	private void txtProxy_TextChanged(object sender, EventArgs e)
	{
		List<string> proxyTxt = (from s in txtProxy.Text.Split("\r\n").ToList()
			where s != ""
			select s).ToList();
		Config config = Files.GetConfig();
		config.proxys = proxyTxt;
		ConfigSetting.PROXYS = proxyTxt;
		Files.SaveConfig(config);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.txtProxy = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.txtProxy.Location = new System.Drawing.Point(12, 33);
		this.txtProxy.Multiline = true;
		this.txtProxy.Name = "txtProxy";
		this.txtProxy.Size = new System.Drawing.Size(776, 387);
		this.txtProxy.TabIndex = 0;
		this.txtProxy.TextChanged += new System.EventHandler(txtProxy_TextChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Controls.Add(this.txtProxy);
		base.Name = "FormProxy";
		this.Text = "FormProxy";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

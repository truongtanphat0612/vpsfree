using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ToolCheckInfo.Core.Constants;
using ToolCheckInfo.Core.Models;
using ToolCheckInfo.Infrastructure.Helpers;

namespace ToolCheckInfo.App.UI.Helpers
{
    public static class ConfigUIHelper
    {
        // Đẩy dữ liệu từ Config lên các Control
        public static void LoadConfigToUI(NumericUpDown numThread, CheckBox chkHide, CheckBox chkProxy, TextBox txtBot, TextBox txtId)
        {
            Config config = Files.GetConfig();

            // Validate Thread
            if (config.Threads <= 0) config.Threads = 1;

            numThread.Value = config.Threads;
            chkHide.Checked = config.HideChrome;
            chkProxy.Checked = config.IsProxy;
            txtBot.Text = config.BotTele;
            txtId.Text = config.IDTele;

            // Cập nhật biến toàn cục
            UpdateGlobalConfig(config);
        }

        // Lưu dữ liệu từ Control về File Config
        public static void SaveConfigFromUI(decimal threads, bool hideChrome, bool useProxy, string botToken, string chatId)
        {
            Config config = Files.GetConfig();
            config.Threads = (int)threads;
            if (config.Threads <= 0) config.Threads = 1;

            config.HideChrome = hideChrome;
            config.IsProxy = useProxy;
            config.BotTele = botToken;
            config.IDTele = chatId;

            Files.SaveConfig(config);
            UpdateGlobalConfig(config);
        }

        private static void UpdateGlobalConfig(Config config)
        {
            ConfigSetting.THREADS = config.Threads;
            ConfigSetting.HIDECHROME = config.HideChrome;
            ConfigSetting.BotTele = config.BotTele;
            ConfigSetting.IDTele = config.IDTele;
            ConfigSetting.ISPROXY = config.IsProxy;
            ConfigSetting.PROXYS = config.proxys;
        }
    }
}
using System;
using System.IO;
using System.Windows.Forms;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace ToolCheckInfo.Infrastructure.Helpers
{
    public class DriverUpdater
    {
        public static void ForceUpdateChromeDriver()
        {
            try
            {
                SystemHelper.KillChromeDrivers();
                string driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromedriver.exe");

                if (File.Exists(driverPath))
                {
                    try { File.Delete(driverPath); } catch { }
                }

                new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);

                MessageBox.Show("Cập nhật Driver thành công!", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }
    }
}
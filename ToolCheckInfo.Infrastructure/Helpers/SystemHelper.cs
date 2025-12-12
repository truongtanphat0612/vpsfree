using System;
using System.Diagnostics;

namespace ToolCheckInfo.Infrastructure.Helpers
{
    public class SystemHelper
    {
        public static void KillChromeDrivers()
        {
            try
            {
                // Tìm và diệt tất cả tiến trình chromedriver đang chạy ngầm
                Process[] chromeDriverProcesses = Process.GetProcessesByName("chromedriver");
                foreach (var process in chromeDriverProcesses)
                {
                    try { process.Kill(); } catch { }
                }
            }
            catch { }
        }
    }
}
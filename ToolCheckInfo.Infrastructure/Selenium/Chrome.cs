using System;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ToolCheckInfo.ConstanData;

namespace ToolCheckInfo.Infrastructure.Selenium
{
    public class Chrome
    {
        private string size { get; set; }
        private string name { get; set; }
        private string proxy { get; set; }
        private int x { get; set; }
        private int y { get; set; }
        public ChromeDriver driver { get; set; }
        public string LastError { get; private set; } = "";
        private const string DEFAULT_SIZE = "400,650";

        public Chrome(string name, int x, int y, string size = "", string location = "", string proxy = "", string user_agent = "")
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.size = string.IsNullOrEmpty(size) ? DEFAULT_SIZE : size;
            this.proxy = proxy;
        }

        public void OpenProfile()
        {
            ChromeDriver profile_driver = null;
            try
            {
                ChromeOptions options = new ChromeOptions();
                options.PageLoadStrategy = PageLoadStrategy.Eager;
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-notifications");
                options.AddArgument("--disable-popup-blocking");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddArgument("--window-size=" + size);

                if (!ConfigSetting.HIDECHROME)
                {
                    options.AddArgument("--window-position=" + x + "," + y);
                }

                string profilePath = Path.GetFullPath("Profile");
                if (!Directory.Exists(profilePath)) Directory.CreateDirectory(profilePath);

                if (!string.IsNullOrEmpty(name) && name != "profile_no")
                {
                    options.AddArgument("--user-data-dir=" + profilePath);
                    options.AddArgument("--profile-directory=" + name);
                }

                if (!string.IsNullOrEmpty(proxy) && proxy.Contains(":"))
                {
                    string[] p = proxy.Split(':');
                    if (p.Length >= 2) options.AddArgument($"--proxy-server={p[0]}:{p[1]}");
                }

                if (ConfigSetting.HIDECHROME) options.AddArgument("--headless=new");

                ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                driverService.SuppressInitialDiagnosticInformation = true;

                profile_driver = new ChromeDriver(driverService, options);
                profile_driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                driver = profile_driver;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                if (profile_driver != null) try { profile_driver.Quit(); } catch { }
                driver = null;
            }
        }

        public void ScrollSmooth()
        {
            if (driver == null) return;
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                for (int i = 0; i < 3; i++)
                {
                    js.ExecuteScript("window.scrollBy(0, 400);");
                    Thread.Sleep(300);
                }
            }
            catch { }
        }

        public void Exit()
        {
            if (driver == null) return;
            try { driver.Close(); } catch { }
            try { driver.Quit(); } catch { }
            try { driver.Dispose(); } catch { }
            driver = null;
        }

        public void GoTo(string url)
        {
            if (driver == null) return;
            try { driver.Navigate().GoToUrl(url); } catch { }
        }

        public string GetCookie()
        {
            if (driver == null) return "";
            string cookieString = "";
            try
            {
                var cookies = driver.Manage().Cookies.AllCookies;
                foreach (var cookie in cookies) cookieString += cookie.Name + "=" + cookie.Value + ";";
            }
            catch { }
            return cookieString;
        }
    }
}
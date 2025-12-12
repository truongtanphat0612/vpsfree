using System;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing; // Cho Color
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ToolCheckInfo.Core.Interfaces;
using ToolCheckInfo.Core.Models;
using ToolCheckInfo.Infrastructure.Selenium;

namespace ToolCheckInfo.Infrastructure.Services
{
    public class LoginService : ILoginService
    {
        private readonly Chrome _chrome;

        public LoginService(Chrome chrome)
        {
            _chrome = chrome;
        }

        public async Task<string> LoginAsync(Facebook account, Action<string, Color?> onStatusUpdate = null)
        {
            string status = "Start";
            bool hasCookie = !string.IsNullOrEmpty(account.Cookie);
            bool hasPass = !string.IsNullOrEmpty(account.Password);

            if (hasCookie)
            {
                onStatusUpdate?.Invoke("Đang đăng nhập Cookie...", null);
                LoginWithCookie(account.Cookie);
                status = CheckLoginStatus();
            }
            else
            {
                status = "NoCookie";
            }

            if ((status == "Die" || status == "LoginFail" || status == "NoCookie") && hasPass)
            {
                onStatusUpdate?.Invoke("Cookie lỗi, thử Pass...", Color.Orange);

                string[] passwords = account.Password.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string pass in passwords)
                {
                    onStatusUpdate?.Invoke($"Thử Pass: {pass}", null);
                    LoginWithPassword(account.Uid, pass.Trim(), onStatusUpdate);
                    status = CheckLoginStatus();

                    if (status == "OK" || status == "Checkpoint" || status == "Disabled")
                        break;
                }
            }

            if (status != "OK" && status != "Checkpoint" && status != "Disabled")
            {
                return "Sai Cookie|Uid|Password";
            }

            return status;
        }

        private void LoginWithCookie(string cookie)
        {
            try
            {
                _chrome.GoTo("https://www.facebook.com/");
                string[] cookies = cookie.Split(';');
                foreach (string c in cookies)
                {
                    if (!string.IsNullOrEmpty(c) && c.Contains("="))
                    {
                        string[] temp = c.Split('=');
                        if (temp.Length > 1)
                        {
                            try
                            {
                                _chrome.driver.Manage().Cookies.AddCookie(new Cookie(temp[0].Trim(), temp[1].Trim(), ".facebook.com", "/", DateTime.Now.AddYears(10)));
                            }
                            catch { }
                        }
                    }
                }
                _chrome.driver.Navigate().Refresh();
                Thread.Sleep(2000);
            }
            catch { }
        }

        private void LoginWithPassword(string uid, string pass, Action<string, Color?> onStatusUpdate)
        {
            try
            {
                try { _chrome.driver.Manage().Cookies.DeleteAllCookies(); } catch { }
                Thread.Sleep(500);

                _chrome.GoTo("https://www.facebook.com/login/device-based/regular/login/?login_attempt=1&lwv=100");
                Thread.Sleep(2000);

                HandleAvatarScreen();

                WebDriverWait wait = new WebDriverWait(_chrome.driver, TimeSpan.FromSeconds(10));

                try
                {
                    var userField = wait.Until(d => d.FindElement(By.Id("email")));
                    userField.Clear();
                    HighlightElement(userField);
                    userField.SendKeys(uid);
                    Thread.Sleep(1000);
                }
                catch
                {
                    try
                    {
                        var userFieldParams = _chrome.driver.FindElement(By.Name("email"));
                        HighlightElement(userFieldParams);
                        userFieldParams.Clear();
                        userFieldParams.SendKeys(uid);
                    }
                    catch { }
                }

                try
                {
                    var passField = _chrome.driver.FindElement(By.Id("pass"));
                    if (!passField.Displayed) passField = _chrome.driver.FindElement(By.Name("pass"));

                    HighlightElement(passField);
                    passField.Click();
                    passField.Clear();
                    passField.SendKeys(pass);
                    Thread.Sleep(1000);

                    try
                    {
                        var loginBtn = _chrome.driver.FindElement(By.Name("login"));
                        loginBtn.Click();
                    }
                    catch
                    {
                        var loginBtnXpath = _chrome.driver.FindElement(By.XPath("//button[@name='login']"));
                        loginBtnXpath.Click();
                    }
                }
                catch { }

                Thread.Sleep(3000);
            }
            catch (Exception) { }
        }

        private string CheckLoginStatus()
        {
            try
            {
                Thread.Sleep(2000);
                string url = _chrome.driver.Url;
                string source = _chrome.driver.PageSource;

                if (url.Contains("checkpoint") || source.Contains("checkpoint")) return "Checkpoint";

                if (url.Contains("c_user") || source.Contains("mbasic_logout_button") || source.Contains("Log Out") || source.Contains("Đăng xuất")) return "OK";

                if (source.Contains("incorrect password") || source.Contains("Mật khẩu không chính xác") ||
                    source.Contains("Invalid username") || source.Contains("Sai mật khẩu") ||
                    source.Contains("Bạn đã nhập mật khẩu cũ"))
                    return "WrongPass";

                if (source.Contains("Your account has been disabled") || source.Contains("Tài khoản của bạn đã bị vô hiệu hóa"))
                    return "Disabled";

                return "LoginFail";
            }
            catch { return "Die"; }
        }

        private void HandleAvatarScreen()
        {
            bool isEmailFieldVisible = false;
            try
            {
                if (_chrome.driver.FindElements(By.Id("email")).Count > 0) isEmailFieldVisible = true;
            }
            catch { }

            if (!isEmailFieldVisible)
            {
                string[] escapeXpaths = new string[]
                {
                    "//a[contains(@href, 'add_account')]",
                    "//div[contains(text(), 'Add Account')]",
                    "//div[contains(text(), 'Thêm tài khoản')]",
                    "//a[contains(text(), 'Log into another account')]",
                    "//a[contains(text(), 'Đăng nhập tài khoản khác')]",
                    "//div[@role='button' and contains(@class, '_9lsb')]"
                };

                foreach (string xpath in escapeXpaths)
                {
                    try
                    {
                        var elements = _chrome.driver.FindElements(By.XPath(xpath));
                        if (elements.Count > 0 && elements[0].Displayed)
                        {
                            elements[0].Click();
                            Thread.Sleep(1500);
                            break;
                        }
                    }
                    catch { }
                }
            }
        }

        private void HighlightElement(IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)_chrome.driver).ExecuteScript("arguments[0].style.border='3px solid red'", element);
            }
            catch { }
        }

        public string GetToken()
        {
             try
            {
                _chrome.GoTo("view-source:https://www.facebook.com/dialog/oauth?client_id=124024574287414&redirect_uri=https://www.instagram.com/accounts/signup/&&scope=email&response_type=token");
                Thread.Sleep(1000);
                string src = _chrome.driver.PageSource;
                if (src.Contains("access_token="))
                {
                    string token = src.Split(new string[] { "access_token=" }, StringSplitOptions.None)[1].Split('&')[0];
                    return token;
                }

                _chrome.GoTo("https://www.facebook.com/dialog/oauth?client_id=124024574287414&redirect_uri=https://www.instagram.com/accounts/signup/&&scope=email&response_type=token");
                Thread.Sleep(2000);
                string url = _chrome.driver.Url;
                if (url.Contains("access_token="))
                {
                    string token = url.Split(new string[] { "access_token=" }, StringSplitOptions.None)[1].Split('&')[0];
                    return token;
                }
            }
            catch { }
            return "";
        }
    }
}

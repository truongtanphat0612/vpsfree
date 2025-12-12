using System;
using System.Collections.Generic;
using System.Drawing; // Cho Color
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToolCheckInfo.Core.Interfaces;
using ToolCheckInfo.Core.Models;
using ToolCheckInfo.Infrastructure.Helpers;
using ToolCheckInfo.Infrastructure.Network;
using ToolCheckInfo.Infrastructure.Selenium;

namespace ToolCheckInfo.Infrastructure.Services
{
    public class ScanService : IScanService
    {
        private readonly Chrome _chrome;
        private readonly RestShapeClient _client;

        public ScanService(Chrome chrome, RestShapeClient client)
        {
            _chrome = chrome;
            _client = client;
        }

        public async Task<InforAccount> GetAccountInfoAsync(Facebook account, Action<string, Color?> onStatusUpdate = null)
        {
            try
            {
                // Parse DTSG từ source trước
                ParseDtsg(account);

                // 1. Get Tên
                string name = GetNameFacebook(account, onStatusUpdate);

                // 2. Get Sinh nhật
                string birthday = await GetBirthdayAsync(account, onStatusUpdate);

                // 3. Get Quốc gia (nếu chưa có)
                if (string.IsNullOrEmpty(account.Country))
                {
                    account.Country = await GetCountryAsync(account, onStatusUpdate);
                }

                // 4. Get Tick xanh (Mock theo code cũ)
                string tickGreen = "KhongTichXanh";
                onStatusUpdate?.Invoke("Check Tích Xanh: Không", null);

                // 5. Check Payout
                bool isPayout = await CheckPayoutAsync();
                string payoutStatus = isPayout ? "Payout" : "NoPayout";
                if (isPayout) onStatusUpdate?.Invoke("Payout: Có", null);

                return new InforAccount
                {
                    Uid = account.Uid,
                    Name = name,
                    Birthday = birthday,
                    Cookie = account.Cookie,
                    Country = account.Country,
                    Password = account.Password,
                    Payout = payoutStatus,
                    TickGreen = tickGreen
                };
            }
            catch (Exception ex)
            {
                FileLog.AddDataToFile(FileLog.ConvertErrorToString(ex));
                return new InforAccount { Uid = account.Uid, Name = "Error" };
            }
        }

        private void ParseDtsg(Facebook account)
        {
            try
            {
                string source = _chrome.driver.PageSource;
                var matchDtsg = Regex.Match(source, "\"name\":\"fb_dtsg\",\"value\":\"(.*?)\"");
                if (matchDtsg.Success)
                {
                    account.fb_dtsg = matchDtsg.Groups[1].Value;
                }
                else
                {
                    // Fallback pattern khác
                    var matchDtsg2 = Regex.Match(source, "DTSGInitialData\",\\[\\],{\"token\":\"(.*?)\"");
                    if (matchDtsg2.Success) account.fb_dtsg = matchDtsg2.Groups[1].Value;
                }

                var matchJazoest = Regex.Match(source, "jazoest=(\\d+)");
                account.JAZOEST = matchJazoest.Success ? matchJazoest.Groups[1].Value : "22135";
            }
            catch { }
        }

        private string GetNameFacebook(Facebook account, Action<string, Color?> onStatusUpdate)
        {
            try
            {
                onStatusUpdate?.Invoke("Đang lấy tên...", null);
                string[] source = _chrome.driver.PageSource.Split(new string[] { "CurrentUserInitialData" }, StringSplitOptions.None);
                if (source.Length > 1)
                {
                    string[] source_2 = source[1].Split(new string[] { "DTSGInitialData" }, StringSplitOptions.None);
                    string pattern = "\\{(.*?)\\}";
                    Match matches = Regex.Match(source_2[0], pattern);
                    if (matches.Success)
                    {
                        JObject objectData = JObject.Parse(matches.Value);
                        string name = objectData["NAME"]?.ToString() ?? "Unknown";
                        onStatusUpdate?.Invoke("Tên: " + name, null);

                        string patternx = "c_user=(\\d+)";
                        Match matchxx = Regex.Match(account.Cookie, patternx);
                        if (matchxx.Success) account.Uid = matchxx.Groups[1].Value;
                        else account.Uid = objectData["ACCOUNT_ID"]?.ToString() ?? account.Uid;

                        return name;
                    }
                }
            }
            catch { }
            return "";
        }

        private async Task<string> GetBirthdayAsync(Facebook account, Action<string, Color?> onStatusUpdate)
        {
            try
            {
                onStatusUpdate?.Invoke("Đang lấy sinh nhật...", null);
                var variables = new { @interface = "FB_WEB" };
                string jsonString = JsonConvert.SerializeObject(variables);
                string encodedJsonString = HttpUtility.UrlEncode(jsonString);
                string postData = $"__aaid=0&__a=1&__ccg=EXCELLENT&dpr=1&__comet_req=5&fb_dtsg={HttpUtility.UrlEncode(account.fb_dtsg)}&__spin_b=trunk&fb_api_caller_class=RelayModern&fb_api_req_friendly_name=FXAccountsCenterEditBirthdayDialogQuery&variables={encodedJsonString}&doc_id=7624255020972704";

                string response = await _client.Post("https://www.facebook.com/api/graphql/", postData);
                JObject objectData = JObject.Parse(response);
                JToken birthday = objectData["data"]?["fxcal_settings"]?["node"]?["birthday_shape"];

                if (birthday != null)
                {
                    string bDay = $"{birthday["day"]}-{birthday["month"]}-{birthday["year"]}";
                    onStatusUpdate?.Invoke("Sinh nhật: " + bDay, null);
                    return bDay;
                }
            }
            catch { }
            return "";
        }

        private async Task<string> GetCountryAsync(Facebook account, Action<string, Color?> onStatusUpdate)
        {
            try
            {
                onStatusUpdate?.Invoke("Đang lấy quốc gia...", null);
                string response = await _client.GetAsync("https://www.facebook.com/primary_location/info");

                string[] result_1 = response.Split(new string[] { "\"initialRouteInfo\":" }, StringSplitOptions.None);
                if (result_1.Length > 1)
                {
                    string[] result_2 = result_1[1].Split(new string[] { "\"props\":" }, StringSplitOptions.None);
                    string[] result_3 = result_2[1].Split(new string[] { ",\"entryPoint\":" }, StringSplitOptions.None);
                    JObject objectData = JObject.Parse(result_3[0]);
                    string location = objectData["city"]?.ToString() ?? "";
                    onStatusUpdate?.Invoke("Thành phố: " + location, null);
                    return location;
                }
            }
            catch { }
            return "";
        }

        private async Task<bool> CheckPayoutAsync()
        {
            try
            {
                var response = await _client.GetAsyncCheckEarnMoney("https://www.facebook.com/payout");
                if (response.ResponseUri != null && response.ResponseUri.AbsoluteUri.Contains("overview")) return true;
            }
            catch { }
            return false;
        }

        public async Task<List<Group>> GetGroupsAsync(Facebook account, Action<string, Color?> onStatusUpdate = null)
        {
            return new List<Group>();
        }

        public async Task<List<Page>> GetPagesAsync(Facebook account, Action<string, Color?> onStatusUpdate = null)
        {
            return new List<Page>();
        }

        public async Task<(List<Ads>, List<Bm>)> GetAdsAndBmAsync(Facebook account, Action<string, Color?> onStatusUpdate = null)
        {
             return (new List<Ads>(), new List<Bm>());
        }
    }
}

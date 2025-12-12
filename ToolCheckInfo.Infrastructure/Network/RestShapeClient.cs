using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace ToolCheckInfo.Infrastructure.Network;

public class RestShapeClient
{
    private string UserAgentDefaut = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36";
    private RestClientOptions options;
    public RestClient Client;
    private RestRequest request;

    public RestShapeClient(string cookies)
    {
        Setoptions(null);
        Client = new RestClient(options);
        SetCookie(cookies);
    }

    private void SetCookie(string cookies)
    {
        if (string.IsNullOrEmpty(cookies)) cookies = "locale=vi_VN;";
        string[] array = cookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string coo in array)
        {
            string trimCoo = coo.Trim();
            if (!string.IsNullOrEmpty(trimCoo) && !trimCoo.Contains("useragent=") && !trimCoo.Contains("i_user"))
            {
                int index = trimCoo.IndexOf('=');
                if (index > 0)
                {
                    string name = trimCoo.Substring(0, index);
                    string value = trimCoo.Substring(index + 1);
                    options.CookieContainer.Add(new Cookie(name, value, "/", "facebook.com"));
                    options.CookieContainer.Add(new Cookie(name, value, "/", ".facebook.com"));
                }
            }
        }
    }

    private void Setoptions(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) userAgent = UserAgentDefaut;

        if (options == null)
        {
            options = new RestClientOptions
            {
                Timeout = TimeSpan.FromMilliseconds(60000),
                CookieContainer = new CookieContainer(),
                UserAgent = userAgent,
                Encoding = Encoding.UTF8,
                Expect100Continue = true,
                MaxRedirects = 100,
                FollowRedirects = true,
                ThrowOnAnyError = false,
                FailOnDeserializationError = false,
                ThrowOnDeserializationError = false,
                RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors) => true,
                PreAuthenticate = true
            };
        }
    }

    public async Task<string> GetAsync(string url)
    {
        request = new RestRequest(url);
        AddCommonHeaders();
        request.Method = Method.Get;
        RestResponse res = await Client.ExecuteAsync(request);
        return (res.StatusCode == HttpStatusCode.OK) ? res?.Content : "";
    }

    public async Task<RestResponse> GetAsyncCheckEarnMoney(string url)
    {
        request = new RestRequest(url);
        AddCommonHeaders();
        request.Method = Method.Get;
        return await Client.ExecuteAsync(request);
    }

    public async Task<string> Post(string url, string datapost)
    {
        request = new RestRequest(url);
        AddCommonHeaders();
        request.AddStringBody(datapost, "application/x-www-form-urlencoded");
        request.Method = Method.Post;
        RestResponse res = await Client.ExecuteAsync(request);
        return string.IsNullOrEmpty(res?.Content) ? "" : res?.Content;
    }

    private void AddCommonHeaders()
    {
        request.AddHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
        request.AddHeader("sec-fetch-site", "same-origin");
        request.AddHeader("sec-fetch-dest", "empty");
        request.AddHeader("accept-language", "en-US,en;q=0.9");
        request.AddHeader("sec-fetch-mode", "cors");
        request.AddHeader("referer", "https://www.facebook.com/");
        request.AddHeader("sec-ch-ua-platform", "\"Windows\"");
    }
}
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ToolCheckInfo.Infrastructure.Helpers;

public class TelegramBot
{
	private static readonly HttpClient client = new HttpClient();

	private readonly string botToken;

	private readonly string apiUrl;

	public TelegramBot(string botToken)
	{
		this.botToken = botToken;
		apiUrl = "https://api.telegram.org/bot" + botToken + "/";
	}

	public async Task SendMessageAsync(string chatId, string message)
	{
		string url = $"{apiUrl}sendMessage?chat_id={chatId}&text={message}";
		if (!(await client.GetAsync(url)).IsSuccessStatusCode)
		{
		}
	}

	public async Task SendFileAsync(string chatId, string filePath, string caption = "")
	{
		using MultipartFormDataContent form = new MultipartFormDataContent();
		form.Add(new StringContent(chatId), "chat_id");
		form.Add(new StringContent(caption), "caption");
		form.Add(new StreamContent(File.OpenRead(filePath)), "document", Path.GetFileName(filePath));
		string url = apiUrl + "sendDocument";
		if (!(await client.PostAsync(url, form)).IsSuccessStatusCode)
		{
		}
	}
}

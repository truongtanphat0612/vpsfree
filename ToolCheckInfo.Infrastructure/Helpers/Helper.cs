using System;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ToolCheckInfo.HTTPClientBase;

namespace ToolCheckInfo.Infrastructure.Helpers;

public class Helper
{
	public static string GetMacAddress()
	{
		return (from nic in NetworkInterface.GetAllNetworkInterfaces()
			where nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
			select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
	}

	public static string CreateMD5(string input)
	{
		using MD5 md5 = MD5.Create();
		byte[] inputBytes = Encoding.ASCII.GetBytes(input);
		byte[] hashBytes = md5.ComputeHash(inputBytes);
		return Convert.ToHexString(hashBytes);
	}

	public static async Task<string> CheckUser()
	{
		string address = GetMacAddress();
		string Key = CreateMD5(address);
		RestShapeClient restShapeClient = new RestShapeClient("");
		string postDataNextPage = "key_app=" + Key;
		JObject result_data = JObject.Parse((await restShapeClient.Post("http://45.77.248.194/api", postDataNextPage)).Trim());
		if (string.IsNullOrEmpty(result_data["name"].ToString()))
		{
			return "Liên hệ ADMIN để cấp quyền cho KEY|" + Key;
		}
		if (string.IsNullOrEmpty(result_data["expired"].ToString()))
		{
			return "KEY hết hạn, vui long liên hệ ADMIN để gian hạn|" + Key;
		}
		DateTime enteredDate = DateTime.ParseExact(result_data["expired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
		DateTime time = DateTime.Now;
		int totalDay = (enteredDate - time).Days;
		if (totalDay < 0)
		{
			return "KEY hết hạn, vui long liên hệ ADMIN để gian hạn|" + Key;
		}
		return "OK";
	}
}

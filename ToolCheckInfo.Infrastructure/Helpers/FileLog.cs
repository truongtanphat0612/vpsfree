using System;
using System.IO;

namespace ToolCheckInfo.Infrastructure.Helpers;

public class FileLog
{
	private static string RESOURCE = "LogData";

	public static void AddDataToFile(string data)
	{
		if (!Directory.Exists(RESOURCE))
		{
			Directory.CreateDirectory(RESOURCE);
		}
		File.AppendAllText(RESOURCE + "/" + DateTimes.GetDateTime() + ".txt", DateTimes.GetDateTime() + ": " + data + "\n");
	}

	public static string ConvertErrorToString(Exception ex)
	{
		return ex.Message + "\n" + ex.StackTrace;
	}
}

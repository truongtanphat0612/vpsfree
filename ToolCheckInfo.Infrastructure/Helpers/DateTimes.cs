using System;

namespace ToolCheckInfo.Infrastructure.Helpers;

public class DateTimes
{
	public static string GetDateTime(string format = "MM_dd_yyyy_HH_mm")
	{
		return DateTime.Now.ToString(format);
	}
}

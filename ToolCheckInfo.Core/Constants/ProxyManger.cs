using System.Collections.Generic;

namespace ToolCheckInfo.Core.Constants;

public static class ProxyManger
{
	private static DataProxy dataProxy;

	public static void SetProxy(List<string> proxies)
	{
		dataProxy = new DataProxy(proxies);
	}

	public static string GetNextProxy()
	{
		return dataProxy.GetNextProxy();
	}
}

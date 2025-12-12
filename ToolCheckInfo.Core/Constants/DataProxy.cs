using System.Collections.Generic;

namespace ToolCheckInfo.Core.Constants;

public class DataProxy
{
	private List<string> _proxies;

	private int _currentIndex;

	public DataProxy(List<string> proxies)
	{
		_proxies = proxies;
		_currentIndex = 0;
	}

	public string GetNextProxy()
	{
		if (_proxies.Count == 0)
		{
			return null;
		}
		string nextProxy = _proxies[_currentIndex];
		_currentIndex = (_currentIndex + 1) % _proxies.Count;
		return nextProxy;
	}
}

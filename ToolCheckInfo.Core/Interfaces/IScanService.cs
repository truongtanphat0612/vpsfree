using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using ToolCheckInfo.Core.Models;

namespace ToolCheckInfo.Core.Interfaces
{
    public interface IScanService
    {
        Task<InforAccount> GetAccountInfoAsync(Facebook account, Action<string, Color?> onStatusUpdate = null);
        Task<List<Group>> GetGroupsAsync(Facebook account, Action<string, Color?> onStatusUpdate = null);
        Task<List<Page>> GetPagesAsync(Facebook account, Action<string, Color?> onStatusUpdate = null);
        Task<(List<Ads>, List<Bm>)> GetAdsAndBmAsync(Facebook account, Action<string, Color?> onStatusUpdate = null);
    }
}

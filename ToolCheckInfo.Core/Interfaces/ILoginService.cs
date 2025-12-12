using System;
using System.Drawing;
using System.Threading.Tasks;
using ToolCheckInfo.Core.Models;

namespace ToolCheckInfo.Core.Interfaces
{
    public interface ILoginService
    {
        Task<string> LoginAsync(Facebook account, Action<string, Color?> onStatusUpdate = null);
    }
}

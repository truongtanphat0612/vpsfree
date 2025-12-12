using System.Collections.Generic;

namespace ToolCheckInfo.Core.Constants
{
    public class ConfigSetting
    {
        public static int THREADS = 1;
        public static bool HIDECHROME = false;
        public static string BotTele = "";
        public static string IDTele = "";
        public static bool ISPROXY = false;
        public static List<string> PROXYS = new List<string>();

        // --- THÊM CÁC TÙY CHỌN MỚI ---
        public static bool SCAN_ADS = true;   // Mặc định quét Ads
        public static bool SCAN_PAGE = false; // Mặc định tắt
        public static bool SCAN_GROUP = false;// Mặc định tắt
    }
}
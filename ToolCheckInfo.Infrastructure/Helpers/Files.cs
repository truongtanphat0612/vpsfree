using System.IO;
using System.Windows.Forms; // Cần thêm cái này để lấy đường dẫn file exe
using Newtonsoft.Json;
using ToolCheckInfo.Core.Models;

namespace ToolCheckInfo.Infrastructure.Helpers
{
    public class Files
    {
        // --- ĐƯỜNG DẪN TẬP TRUNG ---
        public static string APP_PATH = Path.GetDirectoryName(Application.ExecutablePath);
        public static string DATA_FOLDER = Path.Combine(APP_PATH, "Data");
        public static string ACCOUNT_DB = Path.Combine(DATA_FOLDER, "Account.txt");
        // ---------------------------

        private static string RESOURCE = "Result/";

        public static string LIVE = RESOURCE + "Live.txt";
        public static string RESULT = RESOURCE + "KetQua.txt";
        public static string LIST_PAGE = RESOURCE + "List_Page.txt";
        public static string LIST_ADS = RESOURCE + "List_Ads.txt";
        public static string LIST_ADS_Good = RESOURCE + "List_Ads_Good.txt";
        public static string LIST_956 = RESOURCE + "List_Checkpoint_956.txt";
        public static string LIST_681 = RESOURCE + "List_Checkpoint_681.txt";
        public static string LIST_282 = RESOURCE + "List_Checkpoint_282.txt";
        public static string LIST_GROUP = RESOURCE + "List_Group.txt";
        public static string LIST_BM = RESOURCE + "List_BM.txt";

        public static string LIVE_ = "Live.txt";
        public static string RESULT_ = "KetQua.txt";
        public static string LIST_PAGE_ = "List_Page.txt";
        public static string LIST_ADS_ = "List_Ads.txt";
        public static string LIST_ADS_Good_ = "List_Ads_Good.txt";
        public static string LIST_956_ = "List_Checkpoint_956.txt";
        public static string LIST_681_ = "List_Checkpoint_681.txt";
        public static string LIST_282_ = "List_Checkpoint_282.txt";
        public static string LIST_GROUP_ = "List_Group.txt";
        public static string LIST_BM_ = "List_BM.txt";

        public static string CONFIG = "Config/";

        public static void AddDataToFile(string source, string data)
        {
            try
            {
                string directory = Path.GetDirectoryName(source);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.AppendAllText(source, data);
            }
            catch { }
        }

        public static void SaveConfig(Config config)
        {
            try
            {
                if (!Directory.Exists(CONFIG)) Directory.CreateDirectory(CONFIG);
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                string URL = Path.GetFullPath(CONFIG);
                File.WriteAllText(URL + "\\config.json", json);
            }
            catch { }
        }

        public static Config GetConfig()
        {
            try
            {
                if (!Directory.Exists(CONFIG)) Directory.CreateDirectory(CONFIG);
                string URL1 = Path.GetFullPath(CONFIG + "\\config.json");

                if (!File.Exists(URL1))
                {
                    Config defaultConfig = new Config
                    {
                        Threads = 1,
                        HideChrome = false,
                        proxys = new System.Collections.Generic.List<string>()
                    };
                    string defaultJson = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                    File.WriteAllText(URL1, defaultJson);
                }

                using StreamReader reader = new StreamReader(URL1);
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(json) ?? new Config();
            }
            catch
            {
                return new Config();
            }
        }
    }
}
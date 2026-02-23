using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VRCMuteSync
{
    public class Settings
    {
        public int OscPort { get; set; } = 9001;
        public List<int> ModifierKeys { get; set; } = new List<int>();
        public int MainKey { get; set; } = 0;

        public static Settings Load()
        {
            if (File.Exists("settings.json"))
            {
                try
                {
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json")) ?? new Settings();
                }
                catch { }
            }
            return new Settings();
        }

        public void Save()
        {
            File.WriteAllText("settings.json", JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
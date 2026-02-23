using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VRCMuteSync
{
    public class Settings
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        public int OscPort { get; set; } = 9001;
        public List<int> Hotkeys { get; set; } = [];

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
            File.WriteAllText("settings.json", JsonSerializer.Serialize(this, _jsonOptions));
        }
    }
}
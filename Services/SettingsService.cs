using System;
using System.IO;
using System.Text.Json;

namespace EmojiWifiWindows.Services
{
    public class AppSettings
    {
        public bool EulaAccepted { get; set; } = false;
        public bool AnalyticsEnabled { get; set; } = true;
        public string AnalyticsClientId { get; set; } = string.Empty;
        public string? AnalyticsSessionId { get; set; }
        public DateTime? AnalyticsLastEventTime { get; set; }
    }

    public class SettingsService
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EmojiWifiWindows",
            "settings.json");

        public AppSettings Settings { get; private set; } = new AppSettings();

        public SettingsService()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    Settings = new AppSettings();
                }

                // Ensure client ID is generated and persisted
                if (string.IsNullOrEmpty(Settings.AnalyticsClientId))
                {
                    Settings.AnalyticsClientId = Guid.NewGuid().ToString();
                    SaveSettings(); // Persist immediately to ensure it's never lost
                }
            }
            catch
            {
                Settings = new AppSettings();
                
                // Even in error case, ensure client ID exists and is persisted
                if (string.IsNullOrEmpty(Settings.AnalyticsClientId))
                {
                    Settings.AnalyticsClientId = Guid.NewGuid().ToString();
                    SaveSettings();
                }
            }
        }

        public void SaveSettings()
        {
            try
            {
                string? dir = Path.GetDirectoryName(SettingsPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmojiWifiWindows.Services
{
    public class AnalyticsService
    {
        // Configuration from firebase_config.json
        private const string MeasurementId = "G-B387NLSSJX";
        private const string ApiSecret = "ch411kMtTRW7z_3XEUlmiw";
        private const string Endpoint = "https://www.google-analytics.com/mp/collect";

        private readonly SettingsService _settingsService;
        private readonly HttpClient _httpClient;

        public AnalyticsService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _httpClient = new HttpClient();
        }

        public async Task LogEvent(string eventName, object? parameters = null)
        {
            if (!_settingsService.Settings.AnalyticsEnabled)
            {
                return;
            }

            try
            {
                var payload = new
                {
                    client_id = _settingsService.Settings.AnalyticsClientId,
                    events = new[]
                    {
                        new
                        {
                            name = eventName,
                            @params = parameters ?? new { }
                        }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                string url = $"{Endpoint}?measurement_id={MeasurementId}&api_secret={ApiSecret}";

                await _httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                // Silently fail for analytics to avoid disrupting user experience
                System.Diagnostics.Debug.WriteLine($"Analytics Error: {ex.Message}");
            }
        }
    }
}

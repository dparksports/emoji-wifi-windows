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
                EnsureSession();

                // Create a dictionary for parameters to easily add session info
                var paramsDict = new System.Collections.Generic.Dictionary<string, object?>();
                
                if (parameters != null)
                {
                    foreach (var prop in parameters.GetType().GetProperties())
                    {
                        paramsDict[prop.Name] = prop.GetValue(parameters);
                    }
                }

                // Add session info
                paramsDict["session_id"] = _settingsService.Settings.AnalyticsSessionId ?? string.Empty;
                paramsDict["engagement_time_msec"] = "100"; // Required for user engagement

                var payload = new
                {
                    client_id = _settingsService.Settings.AnalyticsClientId,
                    events = new[]
                    {
                        new
                        {
                            name = eventName,
                            @params = paramsDict
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

        private void EnsureSession()
        {
            var settings = _settingsService.Settings;
            var now = DateTime.UtcNow;

            // Check if session exists and is within 30 minutes
            if (string.IsNullOrEmpty(settings.AnalyticsSessionId) ||
                !settings.AnalyticsLastEventTime.HasValue ||
                (now - settings.AnalyticsLastEventTime.Value).TotalMinutes > 30)
            {
                // Start new session
                // Use a simplified timestamp-based ID or a GUID, commonly a timestamp in seconds is enough but GUID is safer for uniqueness
                // GA4 often uses timestamp for session_id (e.g. 1678901234)
                settings.AnalyticsSessionId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            }

            // Update last event time
            settings.AnalyticsLastEventTime = now;
            _settingsService.SaveSettings();
        }
    }
}

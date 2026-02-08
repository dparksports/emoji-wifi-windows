using EmojiWifiWindows.Models;
using System;

namespace EmojiWifiWindows.Models
{
    public class WifiHistoryEntry
    {
        public string WifiName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public WifiStyle Style { get; set; }
    }
}

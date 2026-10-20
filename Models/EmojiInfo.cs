namespace EmojiWifiWindows.Models
{
    /// <summary>
    /// One entry of the emoji catalog, sourced from Unicode's emoji-test.txt
    /// (see tools/generate_emoji_catalog.py).
    /// </summary>
    public class EmojiInfo
    {
        public string Emoji { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
    }
}

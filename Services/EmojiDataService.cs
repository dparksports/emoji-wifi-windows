using EmojiWifiWindows.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace EmojiWifiWindows.Services
{
    /// <summary>
    /// Serves the emoji pool for WiFi names. The pool is the embedded
    /// EmojiCatalog.json generated from Unicode's emoji-test.txt (release
    /// 14.0, filtered to fully-qualified, non-flag, single-visual emoji —
    /// see tools/generate_emoji_catalog.py), plus a curated combination
    /// list and themed combinations generated from the catalog.
    /// </summary>
    public class EmojiDataService
    {
        /// <summary>802.11 SSIDs are limited to 32 bytes.</summary>
        private const int SsidMaxBytes = 32;

        private const int GeneratedCombosPerCategory = 6;

        // Deterministic seed: generated combinations stay stable across runs,
        // so screenshots, docs, and muscle memory don't shift every launch.
        private static readonly Random ComboRandom = new(0x20261019);
        private static readonly Random PickRandom = new();

        private readonly List<EmojiInfo> _allEmojis;
        private readonly Dictionary<string, EmojiInfo> _emojiIndex;
        private readonly List<string> _categories;
        private readonly List<(string Name, string Emojis)> _curatedCombinations;
        private readonly List<(string Name, string Emojis)> _combinations;

        public EmojiDataService()
        {
            _allEmojis = LoadCatalog();
            _emojiIndex = _allEmojis.ToDictionary(e => e.Emoji, e => e);
            _categories = _allEmojis.Select(e => e.Category).Distinct().ToList();
            _curatedCombinations = LoadCuratedCombinations();
            _combinations = _curatedCombinations.Concat(GenerateThemedCombinations()).ToList();
        }

        public IReadOnlyList<EmojiInfo> AllEmojis => _allEmojis;

        public IReadOnlyList<string> Categories => _categories;

        public int EmojiCount => _allEmojis.Count;

        public List<(string Name, string Emojis)> GetAllCombinations()
        {
            return _combinations;
        }

        public (string Name, string Emojis) GetRandomCombination()
        {
            return _combinations[PickRandom.Next(_combinations.Count)];
        }

        /// <summary>
        /// Picks a random emoji, preferring ones absent from
        /// <paramref name="excludeEmojis"/> (e.g. emojis the user already
        /// used). Falls back to the full pool when everything is excluded.
        /// </summary>
        public string GetRandomSingleEmoji(ICollection<string>? excludeEmojis = null)
        {
            var candidates = _allEmojis;
            if (excludeEmojis is { Count: > 0 })
            {
                var remaining = _allEmojis.Where(e => !excludeEmojis.Contains(e.Emoji)).ToList();
                if (remaining.Count > 0)
                {
                    candidates = remaining;
                }
            }
            return candidates[PickRandom.Next(candidates.Count)].Emoji;
        }

        public string GetRandomLengthEmoji(int min = 1, int max = 4, ICollection<string>? excludeEmojis = null)
        {
            int length = PickRandom.Next(min, max + 1);
            var builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                string next = GetRandomSingleEmoji(excludeEmojis);
                if (Encoding.UTF8.GetByteCount(builder.ToString() + next) > SsidMaxBytes)
                {
                    break;
                }
                builder.Append(next);
            }
            return builder.ToString();
        }

        public string GetDescription(string emoji)
        {
            if (_emojiIndex.TryGetValue(emoji, out var info))
            {
                return char.ToUpper(info.Name[0], CultureInfo.InvariantCulture) + info.Name[1..];
            }
            return "A unique emoji symbol";
        }

        /// <summary>
        /// Returns the catalog emojis that appear anywhere in the given
        /// network names, so the UI can tell "already used" from "new to you".
        /// </summary>
        public HashSet<string> FindUsedEmojis(IEnumerable<string> networkNames)
        {
            var used = new HashSet<string>();
            foreach (var name in networkNames)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                foreach (var info in _allEmojis)
                {
                    if (!used.Contains(info.Emoji) && name.Contains(info.Emoji))
                    {
                        used.Add(info.Emoji);
                    }
                }
            }
            return used;
        }

        private static List<EmojiInfo> LoadCatalog()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith("EmojiCatalog.json", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
            {
                throw new InvalidOperationException(
                    "Embedded emoji catalog not found. Build error: Services/EmojiCatalog.json must be an EmbeddedResource.");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var catalog = JsonSerializer.Deserialize<List<EmojiInfo>>(stream);
            if (catalog == null || catalog.Count == 0)
            {
                throw new InvalidOperationException("Embedded emoji catalog is empty. Regenerate it with tools/generate_emoji_catalog.py.");
            }
            return catalog;
        }

        private List<(string Name, string Emojis)> GenerateThemedCombinations()
        {
            var generated = new List<(string, string)>();
            foreach (var category in _categories)
            {
                var pool = _allEmojis.Where(e => e.Category == category).Select(e => e.Emoji).ToList();
                if (pool.Count < 2)
                {
                    continue;
                }

                int made = 0;
                int attempts = 0;
                while (made < GeneratedCombosPerCategory && attempts < 100)
                {
                    attempts++;
                    var picked = PickDistinct(pool, ComboRandom.Next(2, 4));
                    string emojis = string.Concat(picked);
                    if (Encoding.UTF8.GetByteCount(emojis) > SsidMaxBytes)
                    {
                        continue;
                    }
                    if (_curatedCombinations.Any(c => c.Emojis == emojis))
                    {
                        continue;
                    }
                    string name = string.Join(" + ", picked.Select(e => TitleCase(_emojiIndex[e].Name)));
                    generated.Add((name, emojis));
                    made++;
                }
            }
            return generated;
        }

        private List<string> PickDistinct(List<string> pool, int count)
        {
            var available = new List<string>(pool);
            var picked = new List<string>(count);
            while (picked.Count < count && available.Count > 0)
            {
                int index = ComboRandom.Next(available.Count);
                picked.Add(available[index]);
                available.RemoveAt(index);
            }
            return picked;
        }

        private static string TitleCase(string name)
        {
            return string.Join(" ",
                name.Split(' ').Select(w => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(w.ToLowerInvariant())));
        }

        private static List<(string Name, string Emojis)> LoadCuratedCombinations()
        {
            return new List<(string, string)>
            {
                ("Tech Hub", "💻📶🌐"), ("Signal Strong", "📡⚡🔥"), ("Network Master", "🔗💾🎮"), ("Digital Space", "🌐💻📱"),
                ("WiFi Zone", "📶🔗💡"), ("Space Station", "🚀🛰️🌌"), ("Galaxy Network", "🌌⭐🌑"), ("Rocket WiFi", "🚀⚡💨"),
                ("Astronaut Zone", "👨‍🚀🛰️🌌"), ("Cosmic Signal", "⭐🌌📡"), ("Gaming Hub", "🎮🎵🎧"), ("Game Zone", "🎮⚔️🛡️"),
                ("Player One", "🎮👾🤖"), ("Gaming Station", "🎮🎸🎤"), ("Arcade WiFi", "🎮💾🔫"), ("Music Studio", "🎵🎧🎤"),
                ("Rock WiFi", "🎸🤘🎵"), ("Sound Wave", "🎵🌊🎧"), ("Music Zone", "🎤🎸🎵"), ("Audio Hub", "🎧🎵🎤"),
                ("Nature WiFi", "🌲🌻🌱"), ("Forest Signal", "🌲🏞️🌿"), ("Garden Network", "🌻🌱🌿"), ("Tree WiFi", "🌲🌳🌱"),
                ("Natural Zone", "🌿🌻🌱"), ("Food Network", "🍕🍔🍟"), ("Pizza WiFi", "🍕🍕🍕"), ("Burger Zone", "🍔🍟🥤"),
                ("Snack Hub", "🍟🍕🍰"), ("Foodie WiFi", "🍕🍔🍰"), ("Cool Zone", "😎🔥⚡"), ("Stylish WiFi", "😎💎✨"),
                ("Awesome Network", "😎👍🔥"), ("Epic WiFi", "🔥⚡💥"), ("Legendary Zone", "👑⚡🔥"), ("Dark Network", "🖤🌑👻"),
                ("Ghost WiFi", "👻💀🖤"), ("Mystery Zone", "🔮🌑👻"), ("Shadow Network", "🖤🌑👻"), ("Night WiFi", "🌙⭐👻"),
                ("Dark Vader", "🖤🤖⚔️"), ("Fun Zone", "😄🎉🎈"), ("Happy WiFi", "😊🌈✨"), ("Party Network", "🎉🎊🎈"),
                ("Joy Zone", "😄😊🎉"), ("Smile WiFi", "😊💖✨"), ("Cat Zone", "🐱😸🐾"), ("Dog WiFi", "🐶🐕🐾"),
                ("Panda Paradise", "🐼🎋🎍"), ("Animal Kingdom", "🐱🐶🐼"), ("Pet Network", "🐾🐱🐶"), ("Storm WiFi", "⛈️⚡🌧️"),
                ("Sunny Zone", "☀️🌞🌻"), ("Rainbow Network", "🌈☀️🌧️"), ("Weather Hub", "🌤️⛈️🌈"), ("Sky WiFi", "☁️🌤️🌈"),
                ("Love Zone", "💖💕💗"), ("Heart WiFi", "❤️💙💚"), ("Sweet Network", "💖🍰💕"), ("Romance Zone", "💕💖💗"),
                ("Love Hub", "❤️💕💖"), ("Power Zone", "⚡🔥💥"), ("Energy WiFi", "⚡🔋💡"), ("Lightning Fast", "⚡💨🚀"),
                ("Power Hub", "⚡🔥💥"), ("Energy Zone", "🔋⚡💡"), ("Simple WiFi", "✨💫⭐"), ("Clean Zone", "🤍✨💫"),
                ("Pure Network", "🤍💫✨"), ("Minimal WiFi", "✨🤍💫"), ("Clear Zone", "💫✨🤍")
            };
        }
    }
}

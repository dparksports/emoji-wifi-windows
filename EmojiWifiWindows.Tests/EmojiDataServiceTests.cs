using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmojiWifiWindows.Services;
using Xunit;

namespace EmojiWifiWindows.Tests
{
    public class EmojiDataServiceTests
    {
        private static bool ContainsAnyCodePoint(string emoji, IEnumerable<int> codePoints)
        {
            var set = codePoints.ToHashSet();
            return emoji.EnumerateRunes().Any(r => set.Contains(r.Value));
        }

        [Fact]
        public void Catalog_IsLargeAndUnique()
        {
            var service = new EmojiDataService();

            Assert.True(service.EmojiCount >= 1000, $"Pool too small: {service.EmojiCount}");
            var emojis = service.AllEmojis.Select(e => e.Emoji).ToList();
            Assert.Equal(emojis.Count, emojis.Distinct().Count());
        }

        [Fact]
        public void Catalog_EveryEmojiFitsInAnSsid()
        {
            var service = new EmojiDataService();

            foreach (var emoji in service.AllEmojis.Select(e => e.Emoji))
            {
                Assert.InRange(Encoding.UTF8.GetByteCount(emoji), 1, 32);
            }
        }

        [Fact]
        public void Catalog_ExcludesFlagsSkinTonesAndZwjSequences()
        {
            var service = new EmojiDataService();

            // Windows renders flag emoji as letter pairs, so flags are excluded.
            var regionalIndicators = Enumerable.Range(0x1F1E6, 26);
            // Skin-tone modifiers and ZWJ sequences are multi-glyph forms that
            // render as separate emoji on older devices.
            var skinTones = Enumerable.Range(0x1F3FB, 5);

            foreach (var entry in service.AllEmojis)
            {
                Assert.DoesNotContain(0x200D, entry.Emoji.EnumerateRunes().Select(r => r.Value));
                Assert.False(ContainsAnyCodePoint(entry.Emoji, regionalIndicators));
                Assert.False(ContainsAnyCodePoint(entry.Emoji, skinTones));
                Assert.DoesNotContain("Flags", entry.Category);
                Assert.DoesNotContain("Component", entry.Category);
            }
        }

        [Fact]
        public void Catalog_HasUsableCategories()
        {
            var service = new EmojiDataService();

            Assert.True(service.Categories.Count >= 7, $"Expected 7+ categories, got {service.Categories.Count}");
            foreach (var category in service.Categories)
            {
                int count = service.AllEmojis.Count(e => e.Category == category);
                Assert.True(count >= 20, $"Category '{category}' has only {count} emoji");
            }
        }

        [Fact]
        public void GetRandomSingleEmoji_PrefersUnexcluded()
        {
            var service = new EmojiDataService();

            // Exclude all but one emoji: the survivor must be picked every time.
            var survivor = service.AllEmojis[0].Emoji;
            var exclude = service.AllEmojis.Skip(1).Select(e => e.Emoji).ToList();
            for (int i = 0; i < 20; i++)
            {
                Assert.Equal(survivor, service.GetRandomSingleEmoji(exclude));
            }
        }

        [Fact]
        public void GetRandomSingleEmoji_FallsBackToFullPoolWhenAllExcluded()
        {
            var service = new EmojiDataService();

            var everything = service.AllEmojis.Select(e => e.Emoji).ToList();
            var picked = service.GetRandomSingleEmoji(everything);

            Assert.Contains(picked, everything);
        }

        [Fact]
        public void GetRandomLengthEmoji_RespectsSsidByteLimit()
        {
            var service = new EmojiDataService();

            for (int i = 0; i < 200; i++)
            {
                var name = service.GetRandomLengthEmoji(1, 4);
                Assert.InRange(Encoding.UTF8.GetByteCount(name), 1, 32);
            }
        }

        [Fact]
        public void GetAllCombinations_IncludesCuratedAndGenerated()
        {
            var service = new EmojiDataService();

            var combinations = service.GetAllCombinations();
            // 64 curated plus themed combos generated per category.
            Assert.True(combinations.Count > 64, $"Expected curated + generated, got {combinations.Count}");

            var names = combinations.Select(c => c.Name).ToList();
            Assert.Equal(names.Count, names.Distinct().Count());
            Assert.Contains("Tech Hub", names);

            foreach (var combo in combinations)
            {
                Assert.False(string.IsNullOrWhiteSpace(combo.Name));
                Assert.InRange(Encoding.UTF8.GetByteCount(combo.Emojis), 1, 32);
            }
        }

        [Fact]
        public void GetRandomCombination_ReturnsListedCombination()
        {
            var service = new EmojiDataService();

            var list = service.GetAllCombinations();
            for (int i = 0; i < 20; i++)
            {
                var picked = service.GetRandomCombination();
                Assert.Contains(picked, list);
            }
        }

        [Fact]
        public void GetDescription_UsesCatalogName()
        {
            var service = new EmojiDataService();

            var pizza = service.AllEmojis.First(e => e.Name == "pizza");
            Assert.StartsWith("Pizza", service.GetDescription(pizza.Emoji), StringComparison.OrdinalIgnoreCase);

            // Non-catalog emoji (e.g. a ZWJ sequence) still gets a description.
            Assert.Equal("A unique emoji symbol", service.GetDescription("👨‍🚀"));
        }

        [Fact]
        public void FindUsedEmojis_MatchesEmojisInsideNetworkNames()
        {
            var service = new EmojiDataService();

            var names = new List<string> { "🍕🍕🍕", "🌲 My Home WiFi 🌲", "plain-text-network" };
            var used = service.FindUsedEmojis(names);

            Assert.Contains("🍕", used);
            Assert.Contains("🌲", used);
            Assert.DoesNotContain("🍔", used);
        }

        [Fact]
        public void FindUsedEmojis_IgnoresNullAndEmptyNames()
        {
            var service = new EmojiDataService();

            var used = service.FindUsedEmojis(new List<string> { null!, "", "📡" });
            Assert.Single(used);
            Assert.Contains("📡", used);
        }
    }
}

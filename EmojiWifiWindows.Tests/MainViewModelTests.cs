using System;
using System.IO;
using System.Linq;
using EmojiWifiWindows.Models;
using EmojiWifiWindows.Services;
using EmojiWifiWindows.ViewModels;
using Xunit;

namespace EmojiWifiWindows.Tests
{
    /// <summary>
    /// Exercises the emoji picker flow end-to-end at the ViewModel level:
    /// used-emoji detection against history, filtering, the 300-tile cap,
    /// and clicking a tile through SelectEmojiCommand.
    /// </summary>
    public class MainViewModelTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "emojiwifi-vm-tests-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            try { Directory.Delete(_dir, true); } catch { }
        }

        private MainViewModel CreateVm(string historyWifiName = "🍕🍕🍕")
        {
            var history = new HistoryService(_dir);
            history.AddEntry(new WifiHistoryEntry
            {
                WifiName = historyWifiName,
                Password = "password-123",
                Timestamp = new DateTime(2026, 1, 1, 12, 0, 0),
                Style = WifiStyle.SingleEmoji
            });
            return new MainViewModel(history);
        }

        [Fact]
        public void Picker_MarksUsedEmojisFromHistory()
        {
            var vm = CreateVm();
            vm.EmojiSearchText = "pizza"; // narrow past the 300-tile "All categories" cap

            var pizza = vm.FilteredEmojiItems.Single(t => t.Name == "pizza");
            Assert.True(pizza.IsUsed);
            Assert.Contains("of 1364 emojis still unused", vm.EmojiUsageSummary);
        }

        [Fact]
        public void Picker_GridsAreCappedForRendering()
        {
            var vm = CreateVm();

            // "All categories" is capped so the non-virtualized WrapPanel stays responsive.
            Assert.Equal(300, vm.FilteredEmojiItems.Count);
            Assert.StartsWith("Showing 300 of 1364 matches", vm.PickerCountText);
        }

        [Fact]
        public void Picker_CategoryFilter_LimitsToSelectedCategory()
        {
            var vm = CreateVm();

            vm.SelectedEmojiCategory = "Animals & Nature";

            Assert.All(vm.FilteredEmojiItems, t => Assert.Equal("Animals & Nature", t.Category));
            Assert.EndsWith("emojis", vm.PickerCountText);
        }

        [Fact]
        public void Picker_SearchSpansAllCategories()
        {
            var vm = CreateVm();

            vm.SelectedEmojiCategory = "Animals & Nature";
            vm.EmojiSearchText = "pizza";

            Assert.All(vm.FilteredEmojiItems, t => t.Name.Contains("pizza", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(vm.FilteredEmojiItems, t => t.Category == "Food & Drink");
        }

        [Fact]
        public void Picker_HideUsedEmojis_FiltersUsedTiles()
        {
            var vm = CreateVm();

            vm.SelectedEmojiCategory = "Food & Drink";
            vm.HideUsedEmojis = true;

            Assert.DoesNotContain(vm.FilteredEmojiItems, t => t.Emoji == "🍕");
            Assert.All(vm.FilteredEmojiItems, t => Assert.False(t.IsUsed));
        }

        [Fact]
        public void SelectEmojiCommand_SetsWifiNameStyleStatusAndQrCode()
        {
            var vm = CreateVm();
            vm.EmojiSearchText = "pizza";

            var pizza = vm.FilteredEmojiItems.Single(t => t.Name == "pizza");
            vm.SelectEmojiCommand.Execute(pizza);

            Assert.Equal("🍕", vm.GeneratedWifiName);
            Assert.Equal(WifiStyle.SingleEmoji, vm.SelectedStyle);
            Assert.Contains("is now your WiFi name", vm.EmojiPickerStatus);
            Assert.NotNull(vm.QrCodeImage);
        }

        [Fact]
        public void Generate_SingleEmoji_PrefersUnusedEmojis()
        {
            var vm = CreateVm();
            vm.SelectedStyle = WifiStyle.SingleEmoji;

            // 🍕 is the only used emoji, so the generator must steer clear of it.
            for (int i = 0; i < 30; i++)
            {
                vm.GenerateWifiCommand.Execute(null);
                Assert.NotEqual("🍕", vm.GeneratedWifiName);
            }
        }

        [Fact]
        public void Generate_RandomLength_AvoidsUsedEmojiAndRespectsSsidLimit()
        {
            var vm = CreateVm();
            vm.SelectedStyle = WifiStyle.RandomLength;

            for (int i = 0; i < 30; i++)
            {
                vm.GenerateWifiCommand.Execute(null);
                Assert.DoesNotContain("🍕", vm.GeneratedWifiName);
                Assert.InRange(System.Text.Encoding.UTF8.GetByteCount(vm.GeneratedWifiName!), 1, 32);
            }
        }
    }
}

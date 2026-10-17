using System;
using System.IO;
using System.Linq;
using EmojiWifiWindows.Models;
using EmojiWifiWindows.Services;
using Xunit;

namespace EmojiWifiWindows.Tests
{
    public class HistoryServiceTests : IDisposable
    {
        private readonly string _dir = Path.Combine(Path.GetTempPath(), "emojiwifi-tests-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            try { Directory.Delete(_dir, true); } catch { }
        }

        private HistoryService CreateService() => new(_dir);

        private static WifiHistoryEntry Entry(string name, string password, int minuteOffset = 0) => new()
        {
            WifiName = name,
            Password = password,
            Timestamp = new DateTime(2026, 1, 1, 12, 0, 0).AddMinutes(minuteOffset),
            Style = WifiStyle.SingleEmoji
        };

        [Fact]
        public void AddEntry_InsertsNewestFirst()
        {
            var service = CreateService();

            service.AddEntry(Entry("first", "aaaa"));
            service.AddEntry(Entry("second", "bbbb"));

            var entries = service.GetRecentEntries();
            Assert.Equal(2, entries.Count);
            Assert.Equal("second", entries[0].WifiName);
        }

        [Fact]
        public void AddEntry_CollapsesConsecutiveDuplicates()
        {
            var service = CreateService();

            service.AddEntry(Entry("same", "pass", minuteOffset: 0));
            service.AddEntry(Entry("same", "pass", minuteOffset: 5));
            service.AddEntry(Entry("same", "pass", minuteOffset: 10));

            var entries = service.GetRecentEntries();
            Assert.Single(entries);
            Assert.Equal(new DateTime(2026, 1, 1, 12, 10, 0), entries[0].Timestamp);
        }

        [Fact]
        public void AddEntry_TrimsToMaxEntries()
        {
            var service = CreateService();

            for (int i = 0; i < 55; i++)
            {
                service.AddEntry(Entry($"net{i}", $"pass{i}"));
            }

            Assert.Equal(50, service.GetRecentEntries().Count);
            // Newest kept, oldest dropped
            Assert.Equal("net54", service.GetRecentEntries()[0].WifiName);
        }

        [Fact]
        public void RemoveEntry_RemovesMatchingEntry()
        {
            var service = CreateService();
            var target = Entry("keep-a", "aaa");
            service.AddEntry(Entry("keep-b", "bbb"));
            service.AddEntry(target);

            service.RemoveEntry(target);

            var entries = service.GetRecentEntries();
            Assert.Single(entries);
            Assert.Equal("keep-b", entries[0].WifiName);
        }
    }
}

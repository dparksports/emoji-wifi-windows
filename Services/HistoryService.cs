using EmojiWifiWindows.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EmojiWifiWindows.Services
{
    public class HistoryService
    {
        private readonly string _historyPath;
        private const int MaxHistoryEntries = 50;

        public HistoryService()
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EmojiWifiWindows"))
        {
        }

        public HistoryService(string baseDirectory)
        {
            _historyPath = Path.Combine(baseDirectory, "history.json");
        }

        public List<WifiHistoryEntry> LoadHistory()
        {
            try
            {
                if (File.Exists(_historyPath))
                {
                    string json = File.ReadAllText(_historyPath);
                    var history = JsonSerializer.Deserialize<List<WifiHistoryEntry>>(json);
                    return history ?? new List<WifiHistoryEntry>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load history: {ex.Message}");
            }

            return new List<WifiHistoryEntry>();
        }

        public void AddEntry(WifiHistoryEntry entry)
        {
            try
            {
                var history = LoadHistory();

                // Collapse consecutive duplicates so repeated use of the same
                // configuration refreshes its timestamp instead of piling up.
                if (history.Count > 0 &&
                    history[0].WifiName == entry.WifiName &&
                    history[0].Password == entry.Password &&
                    history[0].Style == entry.Style)
                {
                    history[0].Timestamp = entry.Timestamp;
                }
                else
                {
                    history.Insert(0, entry);
                }

                if (history.Count > MaxHistoryEntries)
                {
                    history = history.Take(MaxHistoryEntries).ToList();
                }

                SaveHistory(history);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to add history entry: {ex.Message}");
            }
        }

        public void RemoveEntry(WifiHistoryEntry entryToRemove)
        {
            try
            {
                var history = LoadHistory();
                var item = history.FirstOrDefault(x =>
                    x.WifiName == entryToRemove.WifiName &&
                    x.Timestamp == entryToRemove.Timestamp &&
                    x.Password == entryToRemove.Password);

                if (item != null)
                {
                    history.Remove(item);
                    SaveHistory(history);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to remove history entry: {ex.Message}");
            }
        }

        public List<WifiHistoryEntry> GetRecentEntries(int count = 50)
        {
            var history = LoadHistory();
            return history.Take(count).ToList();
        }

        private void SaveHistory(List<WifiHistoryEntry> history)
        {
            try
            {
                string? dir = Path.GetDirectoryName(_historyPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_historyPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save history: {ex.Message}");
            }
        }
    }
}

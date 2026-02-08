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
        private static readonly string HistoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EmojiWifiWindows",
            "history.json");

        private const int MaxHistoryEntries = 50;

        public List<WifiHistoryEntry> LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryPath))
                {
                    string json = File.ReadAllText(HistoryPath);
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
                
                // Add new entry at the beginning (newest first)
                history.Insert(0, entry);
                
                // Limit to max entries
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
                string? dir = Path.GetDirectoryName(HistoryPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(HistoryPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save history: {ex.Message}");
            }
        }
    }
}

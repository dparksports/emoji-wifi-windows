using CommunityToolkit.Mvvm.ComponentModel;

namespace EmojiWifiWindows.ViewModels
{
    /// <summary>
    /// One emoji tile in the Emoji Picker grid.
    /// </summary>
    public partial class EmojiItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isUsed;

        public string Emoji { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string TooltipText => IsUsed ? $"{Name} — already used" : Name;

        partial void OnIsUsedChanged(bool value) => OnPropertyChanged(nameof(TooltipText));
    }
}

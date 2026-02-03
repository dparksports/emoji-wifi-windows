using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmojiWifiWindows.Models;
using EmojiWifiWindows.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EmojiWifiWindows.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly EmojiDataService _emojiService;
        private readonly PasswordService _passwordService;
        private readonly QrService _qrService;
        private readonly WifiService _wifiService;

        [ObservableProperty]
        private string _generatedWifiName;

        [ObservableProperty]
        private string _generatedPassword;

        [ObservableProperty]
        private string _passwordLengthDescription;

        [ObservableProperty]
        private BitmapImage _qrCodeImage;

        [ObservableProperty]
        private WifiStyle _selectedStyle;

        [ObservableProperty]
        private double _passwordLength = 62;

        [ObservableProperty]
        private bool _includeUpper = true;

        [ObservableProperty]
        private bool _includeLower = true;

        [ObservableProperty]
        private bool _includeNumbers = true;

        [ObservableProperty]
        private bool _includeSpecial = true;

        [ObservableProperty]
        private string _joinStatusMessage;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private List<(string Name, string Emojis)> _combinations;

        [ObservableProperty]
        private List<(string Name, string Emojis)> _filteredCombinations;

        public MainViewModel()
        {
            _emojiService = new EmojiDataService();
            _passwordService = new PasswordService();
            _qrService = new QrService();
            _wifiService = new WifiService();

            _combinations = _emojiService.GetAllCombinations();
            FilteredCombinations = _combinations;

            SelectedStyle = WifiStyle.SingleEmoji;

            GenerateWifi(); // Initial generation
        }

        [RelayCommand]
        public void GenerateWifi()
        {
            // 1. Generate Name
            switch (SelectedStyle)
            {
                case WifiStyle.Combination:
                    GeneratedWifiName = _emojiService.GetRandomCombination().Emojis;
                    break;
                case WifiStyle.SingleEmoji:
                    GeneratedWifiName = _emojiService.GetRandomSingleEmoji();
                    break;
                case WifiStyle.RandomLength:
                    GeneratedWifiName = _emojiService.GetRandomLengthEmoji();
                    break;
            }

            // 2. Generate Password
            RegeneratePassword();
        }

        [RelayCommand]
        public void RegeneratePassword()
        {
            GeneratedPassword = _passwordService.GeneratePassword((int)PasswordLength, IncludeUpper, IncludeLower, IncludeNumbers, IncludeSpecial);
            PasswordLengthDescription = $"{(int)PasswordLength} characters";
            UpdateQrCode();
        }

        private void UpdateQrCode()
        {
            if (string.IsNullOrEmpty(GeneratedWifiName) || string.IsNullOrEmpty(GeneratedPassword))
            {
                QrCodeImage = null!;
                return;
            }

            var bytes = _qrService.GenerateWifiQrCode(GeneratedWifiName, GeneratedPassword);
            
            var image = new BitmapImage();
            using (var mem = new MemoryStream(bytes))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            QrCodeImage = image;
        }

        [RelayCommand]
        public void CopyName()
        {
            Clipboard.SetText(GeneratedWifiName ?? string.Empty);
        }

        [RelayCommand]
        public void CopyPassword()
        {
            Clipboard.SetText(GeneratedPassword ?? string.Empty);
        }

        [RelayCommand]
        public async Task JoinWifi()
        {
            JoinStatusMessage = "Connecting...";
            string result = await _wifiService.ConnectToNetwork(GeneratedWifiName ?? string.Empty, GeneratedPassword ?? string.Empty);
            JoinStatusMessage = result;
        }

        [RelayCommand]
        public void ImportQrCode()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
            if (dialog.ShowDialog() == true)
            {
                var result = _qrService.ParseWifiQrCode(dialog.FileName);
                if (result.ssid != null)
                {
                    GeneratedWifiName = result.ssid;
                    GeneratedPassword = result.password ?? "";
                    UpdateQrCode();
                    JoinStatusMessage = "QR Code Imported successfully!";
                }
                else
                {
                    JoinStatusMessage = "Could not find Wi-Fi QR code in image.";
                }
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredCombinations = Combinations;
            }
            else
            {
                FilteredCombinations = Combinations.Where(c => c.Name.ToLower().Contains(value.ToLower()) || c.Emojis.Contains(value)).ToList();
            }
        }

        partial void OnPasswordLengthChanged(double value) => RegeneratePassword();
        partial void OnIncludeUpperChanged(bool value) => RegeneratePassword();
        partial void OnIncludeLowerChanged(bool value) => RegeneratePassword();
        partial void OnIncludeNumbersChanged(bool value) => RegeneratePassword();
        partial void OnIncludeSpecialChanged(bool value) => RegeneratePassword();
    }
}

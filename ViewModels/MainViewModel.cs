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
        private readonly SettingsService _settingsService;
        private readonly AnalyticsService _analyticsService;

        [ObservableProperty]
        private string? _generatedWifiName;

        [ObservableProperty]
        private string? _generatedPassword;

        [ObservableProperty]
        private string? _passwordLengthDescription;

        [ObservableProperty]
        private BitmapImage? _qrCodeImage;

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
        private string _joinStatusMessage = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private List<(string Name, string Emojis)> _combinations;

        [ObservableProperty]
        private List<(string Name, string Emojis)> _filteredCombinations;

        [ObservableProperty]
        private bool _isEulaVisible;

        public MainViewModel()
        {
            _emojiService = new EmojiDataService();
            _passwordService = new PasswordService();
            _qrService = new QrService();
            _wifiService = new WifiService();
            _settingsService = new SettingsService();
            
            // Initialize Analytics
            _analyticsService = new AnalyticsService(_settingsService.Settings.AnalyticsClientId);

            // Check EULA status
            IsEulaVisible = !_settingsService.Settings.EulaAccepted;

            _combinations = _emojiService.GetAllCombinations();
            FilteredCombinations = _combinations;

            SelectedStyle = WifiStyle.SingleEmoji;

            GenerateWifi(); // Initial generation
            
            if (!IsEulaVisible)
            {
                _ = _analyticsService.LogEvent("app_launch");
            }
        }

        [RelayCommand]
        public async Task AcceptEula()
        {
            _settingsService.Settings.EulaAccepted = true;
            _settingsService.SaveSettings();
            IsEulaVisible = false;
            await _analyticsService.LogEvent("eula_accepted");
            await _analyticsService.LogEvent("app_launch");
        }

        [RelayCommand]
        public void DeclineEula()
        {
            Application.Current.Shutdown();
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
            
            _ = _analyticsService.LogEvent("generate_wifi", new { style = SelectedStyle.ToString() });
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
            _ = _analyticsService.LogEvent("copy_name");
        }

        [RelayCommand]
        public void CopyPassword()
        {
            Clipboard.SetText(GeneratedPassword ?? string.Empty);
            _ = _analyticsService.LogEvent("copy_password");
        }

        [RelayCommand]
        public async Task JoinWifi()
        {
            JoinStatusMessage = "Connecting...";
            string result = await _wifiService.ConnectToNetwork(GeneratedWifiName ?? string.Empty, GeneratedPassword ?? string.Empty);
            JoinStatusMessage = result;
            await _analyticsService.LogEvent("join_wifi", new { status = result });
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
                    _ = _analyticsService.LogEvent("import_qr_success");
                }
                else
                {
                    JoinStatusMessage = "Could not find Wi-Fi QR code in image.";
                    _ = _analyticsService.LogEvent("import_qr_failed");
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

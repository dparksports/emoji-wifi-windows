# Emoji Wifi for Windows

**Emoji Wifi** is a fun and modern Windows application that generates emoji-only WiFi names. It is a native port of the [Mac version](https://github.com/dparksports/emoji-wifi-mac-v3), built with WPF and .NET.

![EmojiWifi Screenshot](screenshots/app_screenshot.png)

## Features

- **Generate Emoji Names**:
  - **Combination**: Curated themed emoji combinations (e.g., "Space Station" 🚀🛰️🌌).
  - **Single Emoji**: Ultra-minimal names like 📶.
  - **Random Length**: Random emoji strings.
- **Password Generation**: Customizable password generation (length, charset).
- **QR Code Support**: 
  - Generate WiFi QR codes to share globally.
  - Import QR codes from images to retrieve credentials.
- **Wi-Fi Connection**: Simulate joining a network (uses `netsh` integration logic).
- **Modern UI**: Clean WPF interface inspired by the original Mac application.

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later.
- Windows 10/11.

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/emoji-wifi-windows.git
   ```
2. Navigate to the project directory:
   ```bash
   cd emoji-wifi-windows
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

## Development

The project is built using:
- **WPF** (Windows Presentation Foundation)
- **CommunityToolkit.Mvvm** for MVVM pattern
- **QRCoder** for QR code generation
- **ZXing.Net** for QR code scanning/importing

## License

This project is licensed under the Apache 2.0 License - see the [LICENSE](LICENSE) file for details.

## Credits

Original Mac Application by [dparksports](https://github.com/dparksports).
Ported to Windows by Antigravity.

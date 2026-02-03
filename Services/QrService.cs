using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;

namespace EmojiWifiWindows.Services
{
    public class QrService
    {
        public byte[] GenerateWifiQrCode(string ssid, string password)
        {
            // Format: WIFI:T:WPA;S:ssid;P:password;H:false;;
            string payload = $"WIFI:T:WPA;S:{ssid};P:{password};H:false;;";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M))
                {
                    using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                    {
                        return qrCode.GetGraphic(20);
                    }
                }
            }
        }

        public (string ssid, string password) ParseWifiQrCode(string filePath)
        {
            try
            {
                var reader = new BarcodeReader(); // Uses System.Drawing.Bitmap by default in ZXing.Net.Bindings.Windows.Compatibility or default package if on Windows
                // Note: We might need to handle Bitmap loading specifically depending on the exact ZXing package. 
                // Assuming standard ZXing.Net with System.Drawing support for now.
                
                using (var bitmap = (Bitmap)Bitmap.FromFile(filePath))
                {
                    var result = reader.Decode(bitmap);
                    if (result != null)
                    {
                        return ParseWifiPayload(result.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR Parse Error: {ex.Message}");
            }
            return (null, null);
        }

        private (string ssid, string password) ParseWifiPayload(string payload)
        {
            // Expected: WIFI:T:WPA;S:ssid;P:password;H:false;;
            string ssid = null;
            string password = null;

            if (payload.StartsWith("WIFI:"))
            {
                var parts = payload.Replace("WIFI:", "").Split(';');
                foreach (var part in parts)
                {
                    if (part.StartsWith("S:"))
                    {
                        ssid = part.Substring(2);
                    }
                    else if (part.StartsWith("P:"))
                    {
                        password = part.Substring(2);
                    }
                }
            }

            return (ssid, password);
        }
    }
}

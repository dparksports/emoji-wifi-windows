using QRCoder;
using System;
using System.Drawing;
using System.IO;
using ZXing;
using ZXing.Windows.Compatibility;

namespace EmojiWifiWindows.Services
{
    public class QrService
    {
        public byte[] GenerateWifiQrCode(string ssid, string password)
        {
            string payload = WifiQrPayload.Build(ssid, password);

            using QRCodeGenerator qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        public (string? ssid, string? password) ParseWifiQrCode(string filePath)
        {
            try
            {
                var reader = new BarcodeReader();
                using (var bitmap = (Bitmap)Bitmap.FromFile(filePath))
                {
                    var result = reader.Decode(bitmap);
                    if (result != null)
                    {
                        return WifiQrPayload.Parse(result.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR Parse Error: {ex.Message}");
            }
            return (null, null);
        }
    }
}

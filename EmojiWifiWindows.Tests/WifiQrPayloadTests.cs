using EmojiWifiWindows.Services;
using Xunit;

namespace EmojiWifiWindows.Tests
{
    public class WifiQrPayloadTests
    {
        [Fact]
        public void Build_EscapesSpecialCharacters()
        {
            string payload = WifiQrPayload.Build("a;b:c", "p\\d,e\"f");
            Assert.Equal("WIFI:T:WPA;S:a\\;b\\:c;P:p\\\\d\\,e\\\"f;H:false;", payload);
        }

        [Fact]
        public void Parse_RoundTripsSpecialCharacters()
        {
            string ssid = "net;work:\"name\"";
            string password = "pa\\ss;wo:rd,123!";

            var (parsedSsid, parsedPassword) = WifiQrPayload.Parse(WifiQrPayload.Build(ssid, password));

            Assert.Equal(ssid, parsedSsid);
            Assert.Equal(password, parsedPassword);
        }

        [Fact]
        public void Parse_RoundTripsEmojiSsid()
        {
            var (ssid, password) = WifiQrPayload.Parse(WifiQrPayload.Build("🍕🎉🚀", "correct horse battery"));

            Assert.Equal("🍕🎉🚀", ssid);
            Assert.Equal("correct horse battery", password);
        }

        [Fact]
        public void Parse_ReadsPasswordContainingColonAndComma()
        {
            // Regression: these characters are in the generated password charset and
            // previously broke the naive semicolon split.
            var (ssid, password) = WifiQrPayload.Parse("WIFI:T:WPA;S:home;P:ab:cd,ef;;");

            Assert.Equal("home", ssid);
            Assert.Equal("ab:cd,ef", password);
        }

        [Fact]
        public void Parse_ReturnsNulls_ForNonWifiPayload()
        {
            var (ssid, password) = WifiQrPayload.Parse("https://example.com");

            Assert.Null(ssid);
            Assert.Null(password);
        }

        [Fact]
        public void Parse_IgnoresUnknownFields_AndKeepsValues()
        {
            var (ssid, password) = WifiQrPayload.Parse("WIFI:T:WPA;S:cafe;P:secret;H:true;X:extra;");

            Assert.Equal("cafe", ssid);
            Assert.Equal("secret", password);
        }
    }
}

using System.Linq;
using System.Xml.Linq;
using EmojiWifiWindows.Services;
using Xunit;

namespace EmojiWifiWindows.Tests
{
    public class WifiServiceTests
    {
        [Fact]
        public void GenerateProfileXml_EscapesXmlSpecialCharacters()
        {
            // Regression: generated passwords can contain < > & which previously
            // produced invalid profile XML and made "netsh wlan add profile" fail.
            string ssid = "name<1>&\"2\"";
            string password = "pass&<word>\"'";

            string xml = WifiService.GenerateProfileXml(ssid, password);

            XDocument doc = XDocument.Parse(xml); // throws if not well-formed
            // The WLAN profile declares a default namespace, so match elements by local name.
            Assert.Equal(ssid, doc.Descendants().First(e => e.Name.LocalName == "SSID")
                .Elements().First(e => e.Name.LocalName == "name").Value);
            Assert.Equal(password, doc.Descendants().First(e => e.Name.LocalName == "keyMaterial").Value);
        }

        [Fact]
        public void GenerateProfileXml_UsesWpa2Aes()
        {
            string xml = WifiService.GenerateProfileXml("🍕", "12345678");

            Assert.Contains("<authentication>WPA2PSK</authentication>", xml);
            Assert.Contains("<encryption>AES</encryption>", xml);
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EmojiWifiWindows.Services
{
    public class WifiService
    {
        public async Task<string> ConnectToNetwork(string ssid, string password)
        {
            try
            {
                // 1. Create Profile XML
                string profileXml = GenerateProfileXml(ssid, password);
                string profilePath = Path.Combine(Path.GetTempPath(), $"WifiProfile_{Guid.NewGuid()}.xml");
                File.WriteAllText(profilePath, profileXml);

                // 2. Add Profile
                string addOutput = await RunNetshCommand($"wlan add profile filename=\"{profilePath}\"");
                
                // Cleanup XML
                if (File.Exists(profilePath)) File.Delete(profilePath);

                if (!addOutput.ToLower().Contains("added"))
                {
                    return $"Failed to add profile: {addOutput}";
                }

                // 3. Connect
                string connectOutput = await RunNetshCommand($"wlan connect name=\"{ssid}\"");
                return connectOutput;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task<string> RunNetshCommand(string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process p = new Process())
            {
                p.StartInfo = psi;
                p.Start();
                string output = await p.StandardOutput.ReadToEndAsync();
                string error = await p.StandardError.ReadToEndAsync();
                await p.WaitForExitAsync();
                return string.IsNullOrWhiteSpace(error) ? output.Trim() : $"{output}\nError: {error}".Trim();
            }
        }

        private string GenerateProfileXml(string ssid, string password)
        {
            // Hex encoding for SSID to handle Emojis properly? 
            // Windows Profiles use <name> as the profile name, and <SSID><name> as the SSID name.
            // Emojis work in XML if encoding is UTF-8.
            
            // Note: Wifi passwords in XML are usually protected or plain. 
            // keyType 'passPhrase' with 'protected' false means plain text password.

            string profile = $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
	<name>{ssid}</name>
	<SSIDConfig>
		<SSID>
			<name>{ssid}</name>
		</SSID>
	</SSIDConfig>
	<connectionType>ESS</connectionType>
	<connectionMode>auto</connectionMode>
	<MSM>
		<security>
			<authEncryption>
				<authentication>WPA2PSK</authentication>
				<encryption>AES</encryption>
				<useOneX>false</useOneX>
			</authEncryption>
			<sharedKey>
				<keyType>passPhrase</keyType>
				<protected>false</protected>
				<keyMaterial>{password}</keyMaterial>
			</sharedKey>
		</security>
	</MSM>
</WLANProfile>";
            return profile;
        }
    }
}

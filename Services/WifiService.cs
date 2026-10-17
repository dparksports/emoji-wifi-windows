using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EmojiWifiWindows.Services
{
    public class WifiService
    {
        public async Task<(bool Success, string Message)> ConnectToNetwork(string ssid, string password)
        {
            try
            {
                string profileXml = GenerateProfileXml(ssid, password);
                string profilePath = Path.Combine(Path.GetTempPath(), $"WifiProfile_{Guid.NewGuid()}.xml");
                // BOM so netsh reliably decodes non-ASCII (emoji) SSIDs as UTF-8.
                File.WriteAllText(profilePath, profileXml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

                (int addExitCode, string addOutput) = await RunNetshCommand($"wlan add profile filename=\"{profilePath}\"");

                if (File.Exists(profilePath)) File.Delete(profilePath);

                if (addExitCode != 0)
                {
                    return (false, $"Failed to add profile: {addOutput}");
                }

                (int connectExitCode, string connectOutput) = await RunNetshCommand($"wlan connect name=\"{ssid}\"");
                return connectExitCode == 0
                    ? (true, connectOutput)
                    : (false, $"Failed to connect: {connectOutput}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        internal static string GenerateProfileXml(string ssid, string password)
        {
            // SSID and password come from manual entry / QR import and can contain
            // XML-significant characters (<, >, &, quotes) that must be escaped.
            string name = SecurityElement.Escape(ssid) ?? string.Empty;
            string key = SecurityElement.Escape(password) ?? string.Empty;

            return $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>{name}</name>
    <SSIDConfig>
        <SSID>
            <name>{name}</name>
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
                <keyMaterial>{key}</keyMaterial>
            </sharedKey>
        </security>
    </MSM>
</WLANProfile>";
        }

        private static async Task<(int ExitCode, string Output)> RunNetshCommand(string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process p = new Process { StartInfo = psi })
            {
                p.Start();

                // Drain both pipes concurrently; reading one to completion first
                // can deadlock when the other pipe's buffer fills.
                Task<string> stdoutTask = p.StandardOutput.ReadToEndAsync();
                Task<string> stderrTask = p.StandardError.ReadToEndAsync();
                await Task.WhenAll(stdoutTask, stderrTask);
                await p.WaitForExitAsync();

                string output = string.IsNullOrWhiteSpace(stderrTask.Result)
                    ? stdoutTask.Result.Trim()
                    : $"{stdoutTask.Result.Trim()}\nError: {stderrTask.Result.Trim()}";
                return (p.ExitCode, output);
            }
        }
    }
}

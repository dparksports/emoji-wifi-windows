using System;
using System.Collections.Generic;
using System.Text;

namespace EmojiWifiWindows.Services
{
    /// <summary>
    /// Builds and parses the "WIFI:" QR-code payload per the Wi-Fi Network Config format
    /// (https://github.com/zxing/zxing/wiki/Wi-Fi-Network-Config-Format). Inside the SSID
    /// and password the characters \ ; , : " must be backslash-escaped, and fields are
    /// separated by unescaped semicolons — otherwise phones mis-parse the password.
    /// </summary>
    public static class WifiQrPayload
    {
        public static string Build(string ssid, string password, string authType = "WPA", bool hidden = false)
        {
            string hiddenFlag = hidden ? "true" : "false";
            return $"WIFI:T:{authType};S:{Escape(ssid)};P:{Escape(password)};H:{hiddenFlag};";
        }

        public static (string? Ssid, string? Password) Parse(string payload)
        {
            if (payload is null || !payload.StartsWith("WIFI:", StringComparison.Ordinal))
            {
                return (null, null);
            }

            string? ssid = null;
            string? password = null;

            foreach (string field in Split(payload.Substring(5)))
            {
                // Fields look like "K:value" with a single-letter key.
                if (field.Length < 2 || field[1] != ':')
                {
                    continue;
                }

                string value = Unescape(field.Substring(2));
                switch (field[0])
                {
                    case 'S':
                        ssid = value;
                        break;
                    case 'P':
                        password = value;
                        break;
                }
            }

            return (ssid, password);
        }

        public static string Escape(string value)
        {
            var escaped = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (c == '\\' || c == ';' || c == ',' || c == ':' || c == '"')
                {
                    escaped.Append('\\');
                }
                escaped.Append(c);
            }
            return escaped.ToString();
        }

        private static IEnumerable<string> Split(string body)
        {
            var field = new StringBuilder();
            bool escaped = false;

            foreach (char c in body)
            {
                if (escaped)
                {
                    field.Append(c);
                    escaped = false;
                }
                else if (c == '\\')
                {
                    field.Append(c);
                    escaped = true;
                }
                else if (c == ';')
                {
                    yield return field.ToString();
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            yield return field.ToString();
        }

        private static string Unescape(string value)
        {
            var unescaped = new StringBuilder(value.Length);
            bool escaped = false;

            foreach (char c in value)
            {
                if (escaped)
                {
                    unescaped.Append(c);
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else
                {
                    unescaped.Append(c);
                }
            }

            if (escaped)
            {
                // Trailing lone backslash — keep it verbatim.
                unescaped.Append('\\');
            }

            return unescaped.ToString();
        }
    }
}

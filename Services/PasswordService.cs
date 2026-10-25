using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace EmojiWifiWindows.Services
{
    public class PasswordService
    {
        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        // Excludes \ ; , : " — the Wi-Fi QR format backslash-escapes those, and some
        // QR/phone readers show the escapes literally (e.g. "pa\;ss") instead of
        // the actual password.
        private const string Special = "!@#$%^&*()_+-=[]{}|.<>?";

        public string GeneratePassword(int length, bool includeUpper, bool includeLower, bool includeNumbers, bool includeSpecial)
        {
            if (length < 8) length = 8;
            if (length > 63) length = 63;

            var classes = new List<string>();
            if (includeLower) classes.Add(LowerCase);
            if (includeUpper) classes.Add(UpperCase);
            if (includeNumbers) classes.Add(Digits);
            if (includeSpecial) classes.Add(Special);

            if (classes.Count == 0)
            {
                throw new ArgumentException("At least one character type must be selected.");
            }

            // Guarantee at least one character from every selected class, fill the rest
            // from the union, then shuffle so the guaranteed characters aren't predictable.
            var chars = new List<char>(length);
            foreach (string set in classes)
            {
                chars.Add(set[RandomNumberGenerator.GetInt32(set.Length)]);
            }

            string union = string.Concat(classes);
            while (chars.Count < length)
            {
                chars.Add(union[RandomNumberGenerator.GetInt32(union.Length)]);
            }

            char[] result = chars.ToArray();
            RandomNumberGenerator.Shuffle<char>(result);
            return new string(result);
        }
    }
}

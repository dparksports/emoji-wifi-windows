using System;
using System.Linq;
using System.Text;

namespace EmojiWifiWindows.Services
{
    public class PasswordService
    {
        public string GeneratePassword(int length, bool includeUpper, bool includeLower, bool includeNumbers, bool includeSpecial)
        {
            if (length < 8) length = 8;
            if (length > 63) length = 63;

            StringBuilder chars = new StringBuilder();
            if (includeLower) chars.Append("abcdefghijklmnopqrstuvwxyz");
            if (includeUpper) chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            if (includeNumbers) chars.Append("0123456789");
            if (includeSpecial) chars.Append("!@#$%^&*()_+-=[]{}|;:.,<>?");

            if (chars.Length == 0) return "";

            Random random = new Random();
            StringBuilder result = new StringBuilder();
            string charSet = chars.ToString();

            for (int i = 0; i < length; i++)
            {
                result.Append(charSet[random.Next(charSet.Length)]);
            }

            return result.ToString();
        }
    }
}

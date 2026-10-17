using System;
using System.Linq;
using EmojiWifiWindows.Services;
using Xunit;

namespace EmojiWifiWindows.Tests
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _service = new();

        [Fact]
        public void GeneratePassword_ClampsLengthToWpa2Limits()
        {
            Assert.Equal(8, _service.GeneratePassword(3, true, true, true, true).Length);
            Assert.Equal(63, _service.GeneratePassword(100, true, true, true, true).Length);
        }

        [Fact]
        public void GeneratePassword_ContainsAtLeastOneCharacterPerSelectedClass()
        {
            for (int i = 0; i < 100; i++)
            {
                string password = _service.GeneratePassword(12, true, true, true, true);

                Assert.True(password.Any(char.IsUpper), $"Missing uppercase in '{password}'");
                Assert.True(password.Any(char.IsLower), $"Missing lowercase in '{password}'");
                Assert.True(password.Any(char.IsDigit), $"Missing digit in '{password}'");
                Assert.True(password.Any(c => !char.IsLetterOrDigit(c)), $"Missing special in '{password}'");
            }
        }

        [Fact]
        public void GeneratePassword_UsesOnlySelectedClasses()
        {
            string password = _service.GeneratePassword(200, includeUpper: false, includeLower: true, includeNumbers: false, includeSpecial: false);

            Assert.All(password, c => Assert.True(char.IsLower(c)));
        }

        [Fact]
        public void GeneratePassword_NoClassSelected_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _service.GeneratePassword(12, false, false, false, false));
        }
    }
}

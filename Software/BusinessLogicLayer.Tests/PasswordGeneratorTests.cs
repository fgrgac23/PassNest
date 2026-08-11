using BusinessLogicLayer.PasswordGeneration;

namespace BusinessLogicLayer.Tests
{
    public class PasswordGeneratorTests
    {
        private readonly PasswordGenerator generator = new();

        [Fact]
        public void GeneratePassword_NoCategorySelected_ThrowsArgumentException()
        {
            var options = new PasswordOptions { Length = 12 };

            Assert.Throws<ArgumentException>(() => generator.GeneratePassword(options));
        }

        [Fact]
        public void GeneratePassword_RespectsRequestedLength()
        {
            var options = new PasswordOptions { Length = 16, UseUppercase = true, UseLowercase = true, UseDigits = true, UseSpecialChars = true };

            var password = generator.GeneratePassword(options);

            Assert.Equal(16, password.Length);
        }

        [Theory]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public void GeneratePassword_ContainsAtLeastOneCharFromEachSelectedCategory(bool upper, bool lower, bool digits, bool special)
        {
            var options = new PasswordOptions { Length = 10, UseUppercase = upper, UseLowercase = lower, UseDigits = digits, UseSpecialChars = special };

            var password = generator.GeneratePassword(options);

            if (upper) Assert.Contains(password, char.IsUpper);
            if (lower) Assert.Contains(password, char.IsLower);
            if (digits) Assert.Contains(password, char.IsDigit);
            if (special) Assert.Contains(password, c => !char.IsLetterOrDigit(c));
        }

        [Fact]
        public void GeneratePassword_LengthSmallerThanMandatoryCategories_ResultLongerThanRequestedLength()
        {
            var options = new PasswordOptions { Length = 2, UseUppercase = true, UseLowercase = true, UseDigits = true, UseSpecialChars = true };

            var password = generator.GeneratePassword(options);

            Assert.Equal(4, password.Length);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        public void EvaluateStrength_EmptyOrTooShort_ReturnsVrloSlaba(string password)
        {
            Assert.Equal(PasswordStrengthLevel.VrloSlaba, generator.EvaluateStrength(password));
        }

        [Fact]
        public void EvaluateStrength_LongComplexPassword_ReturnsJaka()
        {
            Assert.Equal(PasswordStrengthLevel.Jaka, generator.EvaluateStrength("Kx9#mZq2!pLw7@Rt"));
        }

        [Fact]
        public void EvaluateStrength_ShortAllLowercase_ReturnsSlabaOrLower()
        {
            var result = generator.EvaluateStrength("abcdefg");

            Assert.True(result is PasswordStrengthLevel.VrloSlaba or PasswordStrengthLevel.Slaba);
        }

        [Fact]
        public void EvaluateStrength_MixedCaseTwelveChars_ReturnsSrednja()
        {
            Assert.Equal(PasswordStrengthLevel.Srednja, generator.EvaluateStrength("Abcdefghijkl"));
        }
    }
}
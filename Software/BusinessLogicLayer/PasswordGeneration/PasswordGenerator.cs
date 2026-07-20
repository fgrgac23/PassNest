using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.PasswordGeneration
{
    public class PasswordGenerator : IPasswordGenerator
    {
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string DigitChars = "0123456789";
        private const string SpecialChars = "!@#$%&*()-_=+[]{}";

        public string GeneratePassword(PasswordOptions options)
        {
            if(!options.UseUppercase && !options.UseLowercase && !options.UseDigits && !options.UseSpecialChars)
            {
                throw new ArgumentException("Odaberite barem jednu kategoriju znakova.", nameof(options));
            }

            var allowedChars = new StringBuilder();
            var result = new List<Char>();

            if (options.UseUppercase)
            {
                allowedChars.Append(UppercaseChars);
                result.Add(RandomChar(UppercaseChars));
            }

            if (options.UseLowercase)
            {
                allowedChars.Append(LowercaseChars);
                result.Add(RandomChar(LowercaseChars));
            }

            if (options.UseDigits)
            {
                allowedChars.Append(DigitChars);
                result.Add(RandomChar(DigitChars));
            }

            if (options.UseSpecialChars)
            {
                allowedChars.Append(SpecialChars);
                result.Add(RandomChar(SpecialChars));
            }

            var allowed = allowedChars.ToString();
            var missing = options.Length - result.Count;

            for(var i = 0; i < missing; i++)
            {
                result.Add(RandomChar(allowed));
            }

            Shuffle(result);

            return new string(result.ToArray());
        }
        public PasswordStrengthLevel EvaluateStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return PasswordStrengthLevel.VrloSlaba;
            }

            var score = 0;

            if (password.Length >= 12)
            {
                score += 2;
            }

            if (password.Length >= 8)
            {
                score += 1;
            }

            if (password.Any(Char.IsLower))
            {
                score++;
            }

            if (password.Any(Char.IsUpper))
            {
                score++;
            }

            if (password.Any(Char.IsDigit))
            {
                score++;
            }

            if (password.Any(c => !char.IsLetterOrDigit(c)))
            {
                score++;
            }

            return score switch
            {
                <= 1 => PasswordStrengthLevel.VrloSlaba,
                <= 3 => PasswordStrengthLevel.Slaba,
                <= 5 => PasswordStrengthLevel.Srednja,
                _ => PasswordStrengthLevel.Jaka
            };
        }

        private static char RandomChar(string pool) => pool[RandomNumberGenerator.GetInt32(pool.Length)];

        private static void Shuffle(List<Char> chars)
        {
            for(var i = chars.Count - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
        }
    }
}

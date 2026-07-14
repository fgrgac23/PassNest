using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.PasswordGeneration
{
    public class PasswordGenerator : IPasswordGenerator
    {
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
    }
}

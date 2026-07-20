using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.PasswordGeneration
{
    public interface IPasswordGenerator
    {
        string GeneratePassword(PasswordOptions options);
        PasswordStrengthLevel EvaluateStrength(string password);
    }
}

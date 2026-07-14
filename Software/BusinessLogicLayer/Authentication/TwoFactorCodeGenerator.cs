using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Authentication
{
    public class TwoFactorCodeGenerator
    {
        private readonly int CodeLength;

        public TwoFactorCodeGenerator(int codeLength = 6)
        {
            CodeLength = codeLength;
        }

        public string GenerateCode()
        {
            var max = (int)Math.Pow(10, CodeLength);
            
            return RandomNumberGenerator.GetInt32(0, max).ToString().PadLeft(CodeLength, '0');
        }

        public bool ValidateCode(string input, string expected) => input == expected;
    }
}

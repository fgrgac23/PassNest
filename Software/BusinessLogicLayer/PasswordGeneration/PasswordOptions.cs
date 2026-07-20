using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.PasswordGeneration
{
    public class PasswordOptions
    {
        public int Length { get; set; }
        public bool UseUppercase { get; set; }
        public bool UseLowercase { get; set; }
        public bool UseDigits { get; set; }
        public bool UseSpecialChars { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Authentication
{
    public class AuthResult
    {
        public bool Success { get; init; }
        public bool RequiresTwoFactor { get; init; }
        public string ErrorMessage { get; init; } = string.Empty;

        public static AuthResult Ok(bool requiresTwoFactor = false) => new()
        {
            Success = true,
            RequiresTwoFactor = requiresTwoFactor
        };

        public static AuthResult Fail(string errorMessage ) => new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

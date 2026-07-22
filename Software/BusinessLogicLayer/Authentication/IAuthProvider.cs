using EntityLayer;

namespace BusinessLogicLayer.Authentication
{
    public interface IAuthProvider
    {
        bool HasRegisteredUser();
        AuthResult RegisterUser(string name, string surname, string email, string masterPassword);
        AuthResult Login(string masterPassword);
        void EnableTwoFactor(string email);
        void DisableTwoFactor();
        void ResendTwoFactorCode();
        bool VerifyTwoFactor(string code);
        User? GetCurrentUser();
        byte[]? GetEncryptionKey();
        void Logout();
    }
}

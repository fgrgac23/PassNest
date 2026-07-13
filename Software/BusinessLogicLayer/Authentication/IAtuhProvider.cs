using EntityLayer;

namespace BusinessLogicLayer.Authentication
{
    public interface IAtuhProvider
    {
        bool HasRegisteredUser();
        AuthResult RegisterUser(string name, string surname, string masterPassword);
        AuthResult Login(string masterPassword);
        void EnableTwoFactor(string email);
        bool VerifyTwoFactor(string code);
        User? GetCurrentUser();
        byte[]? GetEncryptionKey();
        void Logout();
    }
}

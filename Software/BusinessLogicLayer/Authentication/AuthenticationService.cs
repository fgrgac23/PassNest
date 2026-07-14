using BusinessLogicLayer.Security;
using DataAccessLayer.Email;
using DataAccessLayer.Repository;
using EntityLayer;

namespace BusinessLogicLayer.Authentication
{
    public class AuthenticationService : IAtuhProvider
    {
        private readonly IRepository<User> UserRepository;
        private readonly ICryptoService Crypto;
        private readonly TwoFactorCodeGenerator TwoFactorCodeGenerator;
        private readonly IEmailSender EmailSender;

        private User? CurrentUser;
        private bool IsAuthenticated;
        private string? PendingTwoFactorCode;
        private DateTime TwoFactorExpiresAt;
        private byte[]? EncryptionKey;

        public AuthenticationService(IRepository<User> userRepository, ICryptoService crypto, TwoFactorCodeGenerator twoFactorCodeGenerator, IEmailSender emailSender)
        {
            UserRepository = userRepository;
            Crypto = crypto;
            TwoFactorCodeGenerator = twoFactorCodeGenerator;
            EmailSender = emailSender;
        }

        public bool HasRegisteredUser() => UserRepository.GetAll().Any();

        public User? GetCurrentUser() => CurrentUser;

        public byte[]? GetEncryptionKey() => EncryptionKey;

        public AuthResult RegisterUser(string name, string surname, string masterPassword)
        {
            if(string.IsNullOrWhiteSpace(masterPassword) || masterPassword.Length < 8)
            {
                return AuthResult.Fail("Master lozinka mora imati barem 8 znakova.");
            }

            if (HasRegisteredUser())
            {
                return AuthResult.Fail("Korisnik je već registriran.");
            }

            var salt = Crypto.GenerateSalt();
            var hash = Crypto.HashPassword(masterPassword, salt);

            var user = new User
            {
                Name = name,
                Surname = surname,
                MasterPasswordHash = hash,
                MasterPasswordSalt = salt,
                Is2FAEnabled = false,
                AutoLockMinutes = 5,
                CreatedAt = DateTime.UtcNow
            };

            UserRepository.Add(user);
            UserRepository.SaveChanges();

            CurrentUser = user;
            EncryptionKey = Crypto.DeriveKey(masterPassword, salt);
            IsAuthenticated = true;

            return AuthResult.Ok();
        }

        public AuthResult Login(string masterPassword)
        {
            var user = UserRepository.GetAll().FirstOrDefault();
            if(user is null)
            {
                return AuthResult.Fail("korisnik nije registriran.");
            }

            if(!Crypto.VerifyPassword(masterPassword, user.MasterPasswordHash, user.MasterPasswordSalt))
            {
                return AuthResult.Fail("Neispravna lozinka.");
            }

            CurrentUser = user;
            EncryptionKey = Crypto.DeriveKey(masterPassword, user.MasterPasswordSalt);

            if (user.Is2FAEnabled)
            {
                PendingTwoFactorCode = TwoFactorCodeGenerator.GenerateCode();
                TwoFactorExpiresAt = DateTime.UtcNow.AddMinutes(5);

                EmailSender.SendEmail(user.Email, "PassNest - kod za prijavu", $"Vaš jednokratni kod za prijavu je: {PendingTwoFactorCode}. Kod vrijedi 5 minuta.");

                IsAuthenticated = false;
                return AuthResult.Ok(requiresTwoFactor: true);
            }

            IsAuthenticated = true;
            return AuthResult.Ok();
        }
        public void EnableTwoFactor(string email)
        {
            if(CurrentUser is null)
            {
                throw new InvalidOperationException("Korisnik mora biti prijavljen.");
            }

            CurrentUser.Email = email;
            CurrentUser.Is2FAEnabled = true;
            UserRepository.Update(CurrentUser);
            UserRepository.SaveChanges();
        }

        public bool VerifyTwoFactor(string code)
        {
            if(CurrentUser is null || PendingTwoFactorCode is null)
            {
                return false;
            }

            if(DateTime.UtcNow > TwoFactorExpiresAt)
            {
                return false;
            }

            var isValid = TwoFactorCodeGenerator.ValidateCode(code, PendingTwoFactorCode);
            if (isValid)
            {
                IsAuthenticated = true;
                PendingTwoFactorCode = null;
            }

            return isValid;
        }

        public void Logout()
        {
            CurrentUser = null;
            EncryptionKey = null;
            IsAuthenticated = false;
            PendingTwoFactorCode = null;
        }
    }
}

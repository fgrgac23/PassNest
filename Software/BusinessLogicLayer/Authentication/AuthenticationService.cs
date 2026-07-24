using BusinessLogicLayer.Security;
using DataAccessLayer.Email;
using DataAccessLayer.Repository;
using EntityLayer;

namespace BusinessLogicLayer.Authentication
{
    public class AuthenticationService : IAuthProvider
    {
        private readonly IRepository<User> UserRepository;
        private readonly IRepository<Category> CategoryRepository;
        private readonly ICryptoService Crypto;
        private readonly TwoFactorCodeGenerator TwoFactorCodeGenerator;
        private readonly IEmailSender EmailSender;

        private User? CurrentUser;
        private bool IsAuthenticated;
        private string? PendingTwoFactorCode;
        private DateTime TwoFactorExpiresAt;
        private byte[]? EncryptionKey;

        public AuthenticationService(IRepository<User> userRepository, IRepository<Category> categoryRepository, ICryptoService crypto, TwoFactorCodeGenerator twoFactorCodeGenerator, IEmailSender emailSender)
        {
            UserRepository = userRepository;
            CategoryRepository = categoryRepository;
            Crypto = crypto;
            TwoFactorCodeGenerator = twoFactorCodeGenerator;
            EmailSender = emailSender;
        }

        public bool HasRegisteredUser() => UserRepository.GetAll().Any();

        public User? GetCurrentUser() => CurrentUser;

        public byte[]? GetEncryptionKey() => EncryptionKey;

        public AuthResult RegisterUser(string name, string surname, string email, string masterPassword)
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
                Email = email,
                MasterPasswordHash = hash,
                MasterPasswordSalt = salt,
                Is2FAEnabled = false,
                AutoLockMinutes = 5,
                CreatedAt = DateTime.UtcNow
            };

            UserRepository.Add(user);
            UserRepository.SaveChanges();

            var defaultCategories = new[]
            {
                new Category { UserId = user.UserId, Name = "Posao", Color = "#7C5CD6", IsSystemDefined = true },
                new Category { UserId = user.UserId, Name = "Osobno", Color = "#2AA26A", IsSystemDefined = true },
                new Category { UserId = user.UserId, Name = "Financije", Color = "#E0952E", IsSystemDefined = true },
                new Category { UserId = user.UserId, Name = "Zabava", Color = "#D6503C", IsSystemDefined = true },
            };

            foreach(var category in defaultCategories)
            {
                CategoryRepository.Add(category);
            }

            CategoryRepository.SaveChanges();
            CurrentUser = user;
            EncryptionKey = Crypto.DeriveKey(masterPassword, salt);
            IsAuthenticated = true;

            return AuthResult.Ok();
        }

        public AuthResult Login(string masterPassword)
        {
            var user = UserRepository.GetAll().FirstOrDefault();
            if (user is null)
            {
                return AuthResult.Fail("korisnik nije registriran.");
            }

            if (!Crypto.VerifyPassword(masterPassword, user.MasterPasswordHash, user.MasterPasswordSalt))
            {
                return AuthResult.Fail("Neispravna lozinka.");
            }

            if (user.Is2FAEnabled)
            {
                var code = TwoFactorCodeGenerator.GenerateCode();
                var expiresAt = DateTime.UtcNow.AddMinutes(5);

                try
                {
                    EmailSender.SendEmail(user.Email, "PassNest - kod za prijavu", $"Vaš jednokratni kod za prijavu je: {code}. Kod vrijedi 5 minuta.");
                }
                catch (Exception)
                {
                    return AuthResult.Fail("Provjerite internetsku vezu za slanje 2FA koda i pokušajte ponovno.");
                }

                CurrentUser = user;
                EncryptionKey = Crypto.DeriveKey(masterPassword, user.MasterPasswordSalt);
                PendingTwoFactorCode = code;
                TwoFactorExpiresAt = expiresAt;
                IsAuthenticated = false;
                return AuthResult.Ok(requiresTwoFactor: true);
            }

            CurrentUser = user;
            EncryptionKey = Crypto.DeriveKey(masterPassword, user.MasterPasswordSalt);
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

        public void DisableTwoFactor()
        {
            if(CurrentUser is null)
            {
                throw new InvalidOperationException("Korisnik mora biti prijavljen.");
            }

            CurrentUser.Is2FAEnabled = false;
            UserRepository.Update(CurrentUser);
            UserRepository.SaveChanges();
        }

        public void SetAutoLockMinutes(int minutes)
        {
            if (CurrentUser is null)
            {
                throw new InvalidOperationException("Korisnik mora biti prijavljen.");
            }

            CurrentUser.AutoLockMinutes = minutes;
            UserRepository.Update(CurrentUser);
            UserRepository.SaveChanges();
        }

        public AuthResult ResendTwoFactorCode()
        {
            if (CurrentUser is null)
            {
                throw new InvalidOperationException("Korisnik mora biti prijavljen.");
            }

            var code = TwoFactorCodeGenerator.GenerateCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            try
            {
                EmailSender.SendEmail(CurrentUser.Email, "PassNest - kod za prijavu", $"Vaš jednokratni kod za prijavu je: {code}. Kod vrijedi 5 minuta.");
            }
            catch (Exception)
            {
                return AuthResult.Fail("Provjerite internetsku vezu za slanje 2FA koda i pokušajte ponovno.");
            }

            PendingTwoFactorCode = code;
            TwoFactorExpiresAt = expiresAt;
            return AuthResult.Ok();
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

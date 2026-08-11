using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Security;
using DataAccessLayer.Email;
using EntityLayer;
using Moq;
using System.Reflection;

namespace BusinessLogicLayer.Tests
{
    public class AuthenticationServiceTests
    {
        private readonly InMemoryRepository<User> userRepository = new(u => u.UserId, (u, id) => u.UserId = id);
        private readonly InMemoryRepository<Category> categoryRepository = new(c => c.CategoryId, (c, id) => c.CategoryId = id);
        private readonly EncryptionEngine crypto = new();
        private readonly TwoFactorCodeGenerator codeGenerator = new();
        private readonly Mock<IEmailSender> emailSender = new();

        private AuthenticationService CreateSut() => new(userRepository, categoryRepository, crypto, codeGenerator, emailSender.Object);

        private static object? GetPrivateField(AuthenticationService sut, string name) =>
            typeof(AuthenticationService).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(sut);

        [Fact]
        public void RegisterUser_PasswordTooShort_ReturnsFail()
        {
            var result = CreateSut().RegisterUser("Filip", "Grgac", "filip@test.com", "kratka");

            Assert.False(result.Success);
        }

        [Fact]
        public void RegisterUser_ValidData_CreatesUserAndFourDefaultCategories()
        {
            var sut = CreateSut();

            var result = sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");

            Assert.True(result.Success);
            Assert.Single(userRepository.Items);
            Assert.Equal(4, categoryRepository.Items.Count);
            Assert.NotNull(sut.GetCurrentUser());
            Assert.NotNull(sut.GetEncryptionKey());
        }

        [Fact]
        public void RegisterUser_AlreadyRegistered_ReturnsFail()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");

            var result = sut.RegisterUser("Netko", "Drugi", "netko@test.com", "DrugaLozinka1!");

            Assert.False(result.Success);
        }

        [Fact]
        public void Login_NoRegisteredUser_ReturnsFail()
        {
            Assert.False(CreateSut().Login("BilaKojaLozinka1!").Success);
        }

        [Fact]
        public void Login_WrongPassword_ReturnsFail()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.Logout();

            Assert.False(sut.Login("KrivaLozinka1!").Success);
        }

        [Fact]
        public void Login_CorrectPasswordNo2FA_Succeeds()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.Logout();

            var result = sut.Login("GlavnaLozinka1!");

            Assert.True(result.Success);
            Assert.False(result.RequiresTwoFactor);
            Assert.NotNull(sut.GetCurrentUser());
        }

        [Fact]
        public void Login_With2FAEnabled_SendsEmailAndRequiresTwoFactor()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.EnableTwoFactor("filip@test.com");
            sut.Logout();

            var result = sut.Login("GlavnaLozinka1!");

            Assert.True(result.Success);
            Assert.True(result.RequiresTwoFactor);
            emailSender.Verify(e => e.SendEmail("filip@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.NotNull(sut.GetCurrentUser());
        }

        [Fact]
        public void Login_With2FAEnabled_EmailSendingFails_ReturnsFailWithoutMutatingState()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.EnableTwoFactor("filip@test.com");
            sut.Logout();
            emailSender.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("Nema interneta"));

            var result = sut.Login("GlavnaLozinka1!");

            Assert.False(result.Success);
            Assert.Null(sut.GetCurrentUser());
            Assert.Null(sut.GetEncryptionKey());
        }

        [Fact]
        public void VerifyTwoFactor_CorrectCode_ReturnsTrue()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.EnableTwoFactor("filip@test.com");
            sut.Logout();
            sut.Login("GlavnaLozinka1!");
            var actualCode = (string)GetPrivateField(sut, "PendingTwoFactorCode")!;

            Assert.True(sut.VerifyTwoFactor(actualCode));
        }

        [Fact]
        public void VerifyTwoFactor_WrongCode_ReturnsFalse()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.EnableTwoFactor("filip@test.com");
            sut.Logout();
            sut.Login("GlavnaLozinka1!");

            Assert.False(sut.VerifyTwoFactor("000000"));
        }

        [Fact]
        public void VerifyTwoFactor_ExpiredCode_ReturnsFalse()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.EnableTwoFactor("filip@test.com");
            sut.Logout();
            sut.Login("GlavnaLozinka1!");
            var actualCode = (string)GetPrivateField(sut, "PendingTwoFactorCode")!;

            typeof(AuthenticationService)
                .GetField("TwoFactorExpiresAt", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(sut, DateTime.UtcNow.AddMinutes(-1));

            Assert.False(sut.VerifyTwoFactor(actualCode));
        }

        [Fact]
        public void ResendTwoFactorCode_EmailSendingFails_ReturnsFail()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.EnableTwoFactor("filip@test.com");
            sut.Logout();
            sut.Login("GlavnaLozinka1!");
            emailSender.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("Nema interneta"));

            Assert.False(sut.ResendTwoFactorCode().Success);
        }

        [Fact]
        public void EnableTwoFactor_NoCurrentUser_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => CreateSut().EnableTwoFactor("filip@test.com"));
        }

        [Fact]
        public void Logout_ClearsCurrentUserAndEncryptionKey()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");

            sut.Logout();

            Assert.Null(sut.GetCurrentUser());
            Assert.Null(sut.GetEncryptionKey());
        }

        [Fact]
        public void DisableTwoFactor_NoCurrentUser_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => CreateSut().DisableTwoFactor());
        }

        [Fact]
        public void DisableTwoFactor_TurnsOff2FA()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");
            sut.EnableTwoFactor("filip@test.com");
            sut.DisableTwoFactor();
            sut.Logout();

            var result = sut.Login("GlavnaLozinka1!");

            Assert.True(result.Success);
            Assert.False(result.RequiresTwoFactor);
        }

        [Fact]
        public void SetAutoLockMinutes_NoCurrentUser_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => CreateSut().SetAutoLockMinutes(5));
        }

        [Fact]
        public void SetAutoLockMinutes_UpdatesCurrentUser()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");

            sut.SetAutoLockMinutes(15);

            Assert.Equal(15, sut.GetCurrentUser()!.AutoLockMinutes);
        }

        [Fact]
        public void HasRegisteredUser_NoUsers_ReturnsFalse()
        {
            Assert.False(CreateSut().HasRegisteredUser());
        }

        [Fact]
        public void HasRegisteredUser_AfterRegister_ReturnsTrue()
        {
            var sut = CreateSut();
            sut.RegisterUser("Filip", "Grgac", "filip@test.com", "GlavnaLozinka1!");

            Assert.True(sut.HasRegisteredUser());
        }

        [Fact]
        public void VerifyTwoFactor_NoPendingCode_ReturnsFalse()
        {
            Assert.False(CreateSut().VerifyTwoFactor("123456"));
        }
    }
}
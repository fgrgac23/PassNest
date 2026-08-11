using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.PasswordAudit;
using BusinessLogicLayer.PasswordGeneration;
using Moq;

namespace BusinessLogicLayer.Tests
{
    public class PasswordAuditorTests
    {
        private readonly Mock<IAccountStore> accountStore = new();
        private readonly Mock<IPasswordGenerator> passwordGenerator = new();

        private PasswordAuditor CreateSut() => new(accountStore.Object, passwordGenerator.Object);

        [Fact]
        public void AuditPasswords_AllStrongPasswords_ReturnsEmptyArray()
        {
            accountStore.Setup(a => a.GetAllCredentials()).Returns(new[]
            {
                new AccountCredentials { AccountId = 1, ServiceName = "Github", Password = "JakaLozinka1!" }
            });
            passwordGenerator.Setup(p => p.EvaluateStrength(It.IsAny<string>())).Returns(PasswordStrengthLevel.Jaka);

            Assert.Empty(CreateSut().AuditPasswords());
        }

        [Fact]
        public void AuditPasswords_WeakPassword_ReturnsEntryWithCorrectReason()
        {
            accountStore.Setup(a => a.GetAllCredentials()).Returns(new[]
            {
                new AccountCredentials { AccountId = 1, ServiceName = "Github", Password = "123456" }
            });
            passwordGenerator.Setup(p => p.EvaluateStrength("123456")).Returns(PasswordStrengthLevel.Slaba);

            var entry = Assert.Single(CreateSut().AuditPasswords());
            Assert.Equal("Github", entry.ServiceName);
            Assert.Equal("Slaba lozinka", entry.Reason);
        }

        [Fact]
        public void AuditPasswords_VrloSlabaPassword_ReturnsEntryWithCorrectReason()
        {
            accountStore.Setup(a => a.GetAllCredentials()).Returns(new[]
            {
                new AccountCredentials { AccountId = 1, ServiceName = "Github", Password = "a" }
            });
            passwordGenerator.Setup(p => p.EvaluateStrength("a")).Returns(PasswordStrengthLevel.VrloSlaba);

            Assert.Equal("Vrlo slaba lozinka", CreateSut().AuditPasswords().Single().Reason);
        }

        [Fact]
        public void AuditPasswords_MediumStrengthPassword_IsNotFlagged()
        {
            accountStore.Setup(a => a.GetAllCredentials()).Returns(new[]
            {
                new AccountCredentials { AccountId = 1, ServiceName = "Github", Password = "srednja123" }
            });
            passwordGenerator.Setup(p => p.EvaluateStrength(It.IsAny<string>())).Returns(PasswordStrengthLevel.Srednja);

            Assert.Empty(CreateSut().AuditPasswords());
        }
    }
}
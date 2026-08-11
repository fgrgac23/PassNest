using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Security;
using EntityLayer;
using Moq;

namespace BusinessLogicLayer.Tests
{
    public class AccountManagerTests
    {
        private readonly InMemoryRepository<Account> accountRepository = new(a => a.AccountId, (a, id) => a.AccountId = id);
        private readonly InMemoryRepository<Category> categoryRepository = new(c => c.CategoryId, (c, id) => c.CategoryId = id);
        private readonly EncryptionEngine crypto = new();
        private readonly Mock<IAuthProvider> authProvider = new();
        private readonly byte[] encryptionKey;
        private readonly User currentUser = new() { UserId = 1, Name = "Filip", Surname = "Grgac", MasterPasswordHash = "h", MasterPasswordSalt = "s" };

        public AccountManagerTests()
        {
            encryptionKey = crypto.DeriveKey("GlavnaLozinka1!", crypto.GenerateSalt());
            authProvider.Setup(a => a.GetCurrentUser()).Returns(currentUser);
            authProvider.Setup(a => a.GetEncryptionKey()).Returns(encryptionKey);
        }

        private AccountManager CreateSut() => new(accountRepository, categoryRepository, crypto, authProvider.Object);

        [Fact]
        public void AddAccount_NoCurrentUser_ThrowsInvalidOperationException()
        {
            authProvider.Setup(a => a.GetCurrentUser()).Returns((User?)null);
            var sut = CreateSut();

            Assert.Throws<InvalidOperationException>(() => sut.AddAccount("Github", "filip", "lozinka123", Array.Empty<int>()));
        }

        [Fact]
        public void AddAccount_StoresAccountWithEncryptedPassword()
        {
            var sut = CreateSut();

            sut.AddAccount("Github", "filip", "TajnaLozinka1!", Array.Empty<int>());

            var stored = accountRepository.Items.Single();
            Assert.Equal("Github", stored.ServiceName);
            Assert.NotEqual("TajnaLozinka1!", stored.EncryptedPassword);
            Assert.Equal("TajnaLozinka1!", crypto.Decrypt(stored.EncryptedPassword, encryptionKey));
        }

        [Fact]
        public void AddAccount_AssignsRequestedCategories()
        {
            categoryRepository.Add(new Category { Name = "Posao", IsSystemDefined = true });
            var sut = CreateSut();

            sut.AddAccount("Github", "filip", "TajnaLozinka1!", new[] { 1 });

            Assert.Single(accountRepository.Items.Single().Categories);
        }

        [Fact]
        public void UpdateAccount_ReEncryptsNewPassword()
        {
            var sut = CreateSut();
            sut.AddAccount("Github", "filip", "StaraLozinka1!", Array.Empty<int>());
            var accountId = accountRepository.Items.Single().AccountId;

            sut.UpdateAccount(accountId, "Github", "filip", "NovaLozinka2!", "github.com", Array.Empty<int>());

            var updated = accountRepository.Items.Single();
            Assert.Equal("NovaLozinka2!", crypto.Decrypt(updated.EncryptedPassword, encryptionKey));
            Assert.Equal("github.com", updated.Url);
        }

        [Fact]
        public void DeleteCategory_SystemDefinedCategory_ReturnsFalseAndDoesNotDelete()
        {
            categoryRepository.Add(new Category { Name = "Posao", IsSystemDefined = true });
            var sut = CreateSut();

            var result = sut.DeleteCategory(1);

            Assert.False(result);
            Assert.Single(categoryRepository.Items);
        }

        [Fact]
        public void DeleteCategory_CategoryInUse_ReturnsFalseAndDoesNotDelete()
        {
            categoryRepository.Add(new Category { Name = "Vlastita", IsSystemDefined = false });
            var sut = CreateSut();
            sut.AddAccount("Github", "filip", "Lozinka1!", new[] { 1 });

            var result = sut.DeleteCategory(1);

            Assert.False(result);
            Assert.Single(categoryRepository.Items);
        }

        [Fact]
        public void DeleteCategory_UnusedCustomCategory_ReturnsTrueAndDeletes()
        {
            categoryRepository.Add(new Category { Name = "Vlastita", IsSystemDefined = false });
            var sut = CreateSut();

            var result = sut.DeleteCategory(1);

            Assert.True(result);
            Assert.Empty(categoryRepository.Items);
        }

        [Fact]
        public void AddCategory_FifthCustomCategory_ReturnsNull()
        {
            var sut = CreateSut();
            for (var i = 0; i < 4; i++)
            {
                Assert.NotNull(sut.AddCategory($"Kategorija{i}", "#000000"));
            }

            Assert.Null(sut.AddCategory("Peta", "#111111"));
        }

        [Fact]
        public void GetCredentials_CorruptedEncryptedPassword_ReturnsNull()
        {
            var sut = CreateSut();
            sut.AddAccount("Github", "filip", "Lozinka1!", Array.Empty<int>());
            var account = accountRepository.Items.Single();
            account.EncryptedPassword = Convert.ToBase64String(new byte[20]);

            Assert.Null(sut.GetCredentials(account.AccountId));
        }

        [Fact]
        public void GetAllCredentials_SkipsAccountsWithCorruptedPassword()
        {
            var sut = CreateSut();
            sut.AddAccount("Github", "filip", "Lozinka1!", Array.Empty<int>());
            sut.AddAccount("Spotify", "filip", "Lozinka2!", Array.Empty<int>());
            accountRepository.Items.First(a => a.ServiceName == "Spotify").EncryptedPassword = Convert.ToBase64String(new byte[20]);

            var credentials = sut.GetAllCredentials().ToList();

            Assert.Single(credentials);
            Assert.Equal("Github", credentials[0].ServiceName);
        }
    }
}
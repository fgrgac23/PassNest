using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.BaseBackup;
using BusinessLogicLayer.Security;
using DataAccessLayer.Backup;
using EntityLayer;
using Moq;

namespace BusinessLogicLayer.Tests
{
    public class InMemoryBackupStore : IBackupStore
    {
        private string? content;

        public string ReadFromFile(string filePath) => content ?? throw new InvalidOperationException("Ništa nije zapisano.");

        public void WriteToFile(string data, string filePath) => content = data;
    }

    public class BackupManagerTests
    {
        private readonly InMemoryRepository<Account> accountRepository = new(a => a.AccountId, (a, id) => a.AccountId = id);
        private readonly InMemoryRepository<Category> categoryRepository = new(c => c.CategoryId, (c, id) => c.CategoryId = id);
        private readonly InMemoryBackupStore backupStore = new();
        private readonly EncryptionEngine crypto = new();
        private readonly Mock<IAuthProvider> authProvider = new();
        private const string OriginalPassword = "StaraGlavnaLozinka1!";
        private readonly User user;

        public BackupManagerTests()
        {
            var salt = crypto.GenerateSalt();
            user = new User { UserId = 1, Name = "Filip", Surname = "Grgac", MasterPasswordHash = crypto.HashPassword(OriginalPassword, salt), MasterPasswordSalt = salt };

            authProvider.Setup(a => a.GetCurrentUser()).Returns(user);
            authProvider.Setup(a => a.GetEncryptionKey()).Returns(crypto.DeriveKey(OriginalPassword, salt));
        }

        private BackupManager CreateSut() => new(accountRepository, categoryRepository, backupStore, authProvider.Object, crypto);

        [Fact]
        public void CreateBackup_ThenRestoreWithSamePassword_RestoresAccountWithCorrectPassword()
        {
            categoryRepository.Add(new Category { Name = "Posao", IsSystemDefined = true, UserId = 1 });
            var key = authProvider.Object.GetEncryptionKey()!;
            accountRepository.Add(new Account
            {
                UserId = 1,
                ServiceName = "Github",
                UserName = "filip",
                EncryptedPassword = crypto.Encrypt("TajnaLozinka1!", key),
                Categories = { categoryRepository.Items.Single() }
            });

            var sut = CreateSut();
            sut.CreateBackup("backup.pnbackup");

            accountRepository.Clear();
            categoryRepository.Clear();

            sut.RestoreBackup("backup.pnbackup", OriginalPassword);

            var restored = accountRepository.Items.Single();
            Assert.Equal("Github", restored.ServiceName);
            Assert.Equal("TajnaLozinka1!", crypto.Decrypt(restored.EncryptedPassword, key));
        }

        [Fact]
        public void RestoreBackup_WrongMasterPassword_ThrowsInvalidOperationException()
        {
            var key = authProvider.Object.GetEncryptionKey()!;
            accountRepository.Add(new Account { UserId = 1, ServiceName = "Github", UserName = "filip", EncryptedPassword = crypto.Encrypt("TajnaLozinka1!", key) });

            var sut = CreateSut();
            sut.CreateBackup("backup.pnbackup");

            var ex = Assert.Throws<InvalidOperationException>(() => sut.RestoreBackup("backup.pnbackup", "SasvimKrivaLozinka1!"));
            Assert.Equal("Neispravna lozinka ili oštećena datoteka.", ex.Message);
        }

        [Fact]
        public void RestoreBackup_ExistingCategoryWithSameName_IsNotDuplicated()
        {
            categoryRepository.Add(new Category { Name = "Vlastita", IsSystemDefined = false, UserId = 1 });
            var key = authProvider.Object.GetEncryptionKey()!;
            accountRepository.Add(new Account
            {
                UserId = 1,
                ServiceName = "Github",
                UserName = "filip",
                EncryptedPassword = crypto.Encrypt("TajnaLozinka1!", key),
                Categories = { categoryRepository.Items.Single() }
            });

            var sut = CreateSut();
            sut.CreateBackup("backup.pnbackup");

            sut.RestoreBackup("backup.pnbackup", OriginalPassword);

            Assert.Single(categoryRepository.Items, c => c.Name == "Vlastita");
        }

        [Fact]
        public void RestoreBackup_DifferentMasterPasswordAtDestination_ReEncryptsWithNewKey()
        {
            var oldKey = authProvider.Object.GetEncryptionKey()!;
            accountRepository.Add(new Account { UserId = 1, ServiceName = "Github", UserName = "filip", EncryptedPassword = crypto.Encrypt("TajnaLozinka1!", oldKey) });

            var sut = CreateSut();
            sut.CreateBackup("backup.pnbackup");

            var newKey = crypto.DeriveKey("NovaGlavnaLozinka2!", crypto.GenerateSalt());
            authProvider.Setup(a => a.GetEncryptionKey()).Returns(newKey);
            accountRepository.Clear();

            sut.RestoreBackup("backup.pnbackup", OriginalPassword);

            var restored = accountRepository.Items.Single();
            Assert.Equal("TajnaLozinka1!", crypto.Decrypt(restored.EncryptedPassword, newKey));
        }

        [Fact]
        public void RestoreBackup_AccountWithCorruptedPassword_IsSkipped()
        {
            var key = authProvider.Object.GetEncryptionKey()!;
            accountRepository.Add(new Account { UserId = 1, ServiceName = "Github", UserName = "filip", EncryptedPassword = crypto.Encrypt("TajnaLozinka1!", key) });
            accountRepository.Add(new Account { UserId = 1, ServiceName = "Oštećeno", UserName = "filip", EncryptedPassword = Convert.ToBase64String(new byte[20]) });

            var sut = CreateSut();
            sut.CreateBackup("backup.pnbackup");
            accountRepository.Clear();

            sut.RestoreBackup("backup.pnbackup", OriginalPassword);

            Assert.Equal("Github", accountRepository.Items.Single().ServiceName);
        }

        [Fact]
        public void RestoreBackup_NoCurrentUser_ThrowsInvalidOperationException()
        {
            var sut = CreateSut();
            sut.CreateBackup("backup.pnbackup");
            authProvider.Setup(a => a.GetCurrentUser()).Returns((User?)null);

            Assert.Throws<InvalidOperationException>(() => sut.RestoreBackup("backup.pnbackup", OriginalPassword));
        }

        [Fact]
        public void RestoreBackup_NoEncryptionKey_ThrowsInvalidOperationException()
        {
            var sut = CreateSut();
            sut.CreateBackup("backup.pnbackup");
            authProvider.Setup(a => a.GetEncryptionKey()).Returns((byte[]?)null);

            Assert.Throws<InvalidOperationException>(() => sut.RestoreBackup("backup.pnbackup", OriginalPassword));
        }
    }
}
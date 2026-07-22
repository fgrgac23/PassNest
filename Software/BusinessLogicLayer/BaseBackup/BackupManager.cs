using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Security;
using DataAccessLayer.Backup;
using DataAccessLayer.Repository;
using EntityLayer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogicLayer.BaseBackup
{
    public class BackupManager : IBackupManager
    {
        private readonly IRepository<Account> accountRepository;
        private readonly IRepository<Category> categoryRepository;
        private readonly IBackupStore backupStore;
        private readonly IAuthProvider authProvider;
        private readonly ICryptoService cryptoService;

        public BackupManager(IRepository<Account> accountRepository, IRepository<Category> categoryRepository, IBackupStore backupStore, IAuthProvider authProvider, ICryptoService cryptoService)
        {
            this.accountRepository = accountRepository;
            this.categoryRepository = categoryRepository;
            this.backupStore = backupStore;
            this.authProvider = authProvider;
            this.cryptoService = cryptoService;
        }

        public void CreateBackup(string filePath)
        {
            var currentUser = authProvider.GetCurrentUser() ?? throw new InvalidOperationException("Korisnik mora biti prijvaljen.");
            var key = authProvider.GetEncryptionKey() ?? throw new InvalidOperationException("Nedostaje enkripcijski ključ.");

            var accounts = accountRepository.GetAll(a => a.Categories);
            var categories = categoryRepository.GetAll();

            var data = ConvertData(accounts, categories);
            var encryptedData = cryptoService.Encrypt(data, key);

            var envelope = new BackupEnvelope
            {
                Salt = currentUser.MasterPasswordSalt,
                EncryptedPayload = encryptedData
            };

            backupStore.WriteToFile(JsonSerializer.Serialize(envelope), filePath);
        }

        public void RestoreBackup(string filePath, string masterPassword)
        {
            var currentUser = authProvider.GetCurrentUser() ?? throw new InvalidOperationException("Korisnik mora biti prijavljen.");
            var newKey = authProvider.GetEncryptionKey() ?? throw new InvalidOperationException("Nedostaje enkripcijski ključ.");

            var envelopeJson = backupStore.ReadFromFile(filePath);
            var envelope = JsonSerializer.Deserialize<BackupEnvelope>(envelopeJson) ?? throw new InvalidOperationException("Datoteka nije valjana sigurnosna kopija.");

            var backupKey = cryptoService.DeriveKey(masterPassword, envelope.Salt);

            string json;
            try
            {
                json = cryptoService.Decrypt(envelope.EncryptedPayload, backupKey);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Neispravna lozinka ili oštećena datoteka.");
            }

            var backup = JsonSerializer.Deserialize<BackupData>(json) ?? throw new InvalidOperationException("Datoteka nije valjana sigurnosna kopija.");

            var existingCategories = categoryRepository.GetAll().ToList();
            var categoryIdMap = new Dictionary<int, int>();

            foreach(var categoryData in backup.Categories)
            {
                if (categoryData.IsSystemDefined)
                {
                    var existing = existingCategories.FirstOrDefault(c => c.IsSystemDefined && c.Name == categoryData.Name);

                    if(existing != null)
                    {
                        categoryIdMap[categoryData.CategoryId] = existing.CategoryId;
                        continue;
                    }
                }

                var category = new Category
                {
                    UserId = currentUser.UserId,
                    Name = categoryData.Name,
                    Color = categoryData.Color,
                    IsSystemDefined = categoryData.IsSystemDefined
                };

                categoryRepository.Add(category);
                categoryRepository.SaveChanges();

                categoryIdMap[categoryData.CategoryId] = category.CategoryId;
            }

            foreach(var accountData in backup.Accounts)
            {
                var account = new Account
                {
                    UserId = currentUser.UserId,
                    ServiceName = accountData.ServiceName,
                    UserName = accountData.UserName,
                    Url = accountData.Url,
                    EncryptedPassword = accountData.EncryptedPassword,
                    CreatedAt = accountData.CreatedAt,
                    UpdatedAt = accountData.UpdatedAt,
                };

                foreach(var oldCategoryId in accountData.CategoryIds)
                {
                    if(categoryIdMap.TryGetValue(oldCategoryId, out var newCategoryId))
                    {
                        var category = categoryRepository.GetById(newCategoryId);
                        if(category != null) account.Categories.Add(category);
                    }
                }

                accountRepository.Add(account);
            }

            accountRepository.SaveChanges();
        }

        private static string ConvertData(IEnumerable<Account> accounts, IEnumerable<Category> categories)
        {
            var backup = new BackupData
            {
                Accounts = accounts.Select(a => new AccountBackupData
                {
                    ServiceName = a.ServiceName,
                    UserName = a.UserName,
                    Url = a.Url,
                    EncryptedPassword = a.EncryptedPassword,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    CategoryIds = a.Categories.Select(c => c.CategoryId).ToList()
                }).ToList(),

                Categories = categories.Select(c => new CategoryBackupData
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Color = c.Color,
                    IsSystemDefined = c.IsSystemDefined
                }).ToList()
            };

            return JsonSerializer.Serialize(backup);
        }

        private class BackupEnvelope
        {
            public string Salt { get; set; } = string.Empty;
            public string EncryptedPayload { get; set; } = string.Empty;
        }

        private class BackupData
        {
            public List<AccountBackupData> Accounts { get; set; } = new();
            public List<CategoryBackupData> Categories { get; set; } = new();
        }

        private class AccountBackupData
        {
            public string ServiceName { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string? Url { get; set; }
            public string EncryptedPassword { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public List<int> CategoryIds { get; set; } = new();
        }

        private class CategoryBackupData
        {
            public int CategoryId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public bool IsSystemDefined { get; set; }
        }
    }
}

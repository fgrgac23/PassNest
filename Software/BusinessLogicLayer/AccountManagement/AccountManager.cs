using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Security;
using DataAccessLayer.Repository;
using EntityLayer;

namespace BusinessLogicLayer.AccountManagement
{
    public class AccountManager : IAccountStore
    {
        private readonly IRepository<Account> accountRepository;
        private readonly IRepository<Category> categoryRepository;
        private readonly ICryptoService crypto;
        private readonly IAuthProvider authProvider;

        public AccountManager(IRepository<Account> accountRepository, IRepository<Category> categoryRepository, ICryptoService crypto, IAuthProvider authProvider)
        {
            this.accountRepository = accountRepository;
            this.categoryRepository = categoryRepository;
            this.crypto = crypto;
            this.authProvider = authProvider;
        }

        public void AddAccount(string serviceName, string userName, string password, IEnumerable<int> categoryIds)
        {
            var currentUser = authProvider.GetCurrentUser() ?? throw new InvalidOperationException("Korisnik mora biti prijavljen!");
            var key = authProvider.GetEncryptionKey() ?? throw new InvalidOperationException("Nedostaje enkripcijski ključ!");

            var account = new Account
            {
                UserId = currentUser.UserId,
                ServiceName = serviceName,
                UserName = userName,
                EncryptedPassword = crypto.Encrypt(password, key),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach(var categoryId in categoryIds)
            {
                var category = categoryRepository.GetById(categoryId);
                if(category != null)
                {
                    account.Categories.Add(category);
                }
            }

            accountRepository.Add(account);
            accountRepository.SaveChanges();
        }

        public void UpdateAccount(int accountId, string serviceName, string userName, string password, string? url, IEnumerable<int> categoryIds)
        {
            var account = accountRepository.GetAll(a => a.Categories).FirstOrDefault(a => a.AccountId == accountId);
            if(account == null)
            {
                return;
            }

            var key = authProvider.GetEncryptionKey() ?? throw new InvalidOperationException("Nedostaje enkripcijski ključ!");

            account.ServiceName = serviceName;
            account.UserName = userName;
            account.EncryptedPassword = crypto.Encrypt(password, key);
            account.Url = url;
            account.UpdatedAt = DateTime.UtcNow;

            account.Categories.Clear();
            foreach(var categoryId in categoryIds)
            {
                var category = categoryRepository.GetById(categoryId);
                if(category != null)
                {
                    account.Categories.Add(category);
                }
            }

            accountRepository.Update(account);
            accountRepository.SaveChanges();
        }

        public Category? AddCategory(string name, string color)
        {
            var currentUser = authProvider.GetCurrentUser() ?? throw new InvalidOperationException("Korisnik mora biti prijavljen!");

            var customCategoryCount = categoryRepository.GetAll().Count(c => !c.IsSystemDefined);
            if (customCategoryCount >= 4) return null;

            var category = new Category
            {
                UserId = currentUser.UserId,
                Name = name,
                IsSystemDefined = false,
                Color = color
            };

            categoryRepository.Add(category);
            categoryRepository.SaveChanges();

            return category;
        }

        public bool DeleteCategory(int categoryId)
        {
            var category = categoryRepository.GetById(categoryId);
            if (category == null || category.IsSystemDefined) return false;

            var isInUse = accountRepository.GetAll(a => a.Categories).Any(a => a.Categories.Any(c => c.CategoryId == categoryId));
            if (isInUse) return false;

            categoryRepository.Delete(category);
            categoryRepository.SaveChanges();
            return true;
        }

        public void DeleteAccount(int accountId)
        {
            var account = accountRepository.GetById(accountId);
            if(account == null)
            {
                return;
            }

            accountRepository.Delete(account);
            accountRepository.SaveChanges();
        }

        public IEnumerable<Account> FilterByCategory(int categoryId) => accountRepository.GetAll(a => a.Categories).Where(a => a.Categories.Any(c => c.CategoryId == categoryId));

        public IEnumerable<Account> GetAllAccounts() => accountRepository.GetAll(a => a.Categories);

        public IEnumerable<AccountCredentials> GetAllCredentials()
        {
            var key = authProvider.GetEncryptionKey();
            if (key == null)
            {
                yield break;
            }

            foreach (var account in accountRepository.GetAll())
            {
                string password;
                try
                {
                    password = crypto.Decrypt(account.EncryptedPassword, key);
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    continue;
                }

                yield return new AccountCredentials
                {
                    AccountId = account.AccountId,
                    ServiceName = account.ServiceName,
                    UserName = account.UserName,
                    Password = password
                };
            }
        }

        public IEnumerable<Category> GetCategories() => categoryRepository.GetAll();

        public AccountCredentials? GetCredentials(int accountId)
        {
            var account = accountRepository.GetById(accountId);
            var key = authProvider.GetEncryptionKey();
            if (account == null || key == null)
            {
                return null;
            }

            try
            {
                return new AccountCredentials
                {
                    AccountId = account.AccountId,
                    ServiceName = account.ServiceName,
                    UserName = account.UserName,
                    Password = crypto.Decrypt(account.EncryptedPassword, key)
                };
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return null;
            }
        }

        public IEnumerable<Account> SearchAccounts(string query) => accountRepository.GetAll(a => a.Categories).Where(a => a.ServiceName.Contains(query, StringComparison.OrdinalIgnoreCase));
    }
}

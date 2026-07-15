using EntityLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.AccountManagement
{
    public interface IAccountStore
    {
        void AddAccount(string serviceName, string userName, string password, IEnumerable<int> categoryIds);
        void UpdateAccount(int accountId, string serviceName, string userName, string password, IEnumerable<int> categoryIds);
        void DeleteAccount(int accountId);
        IEnumerable<Account> GetAllAccounts();
        IEnumerable<Account> SearchAccounts(string query);
        IEnumerable<Account> FilterByCategory(int categoryId);
        IEnumerable<Category> GetCategories();
        Category AddCategory(string name, string color);
        AccountCredentials? GetCredentials(int accountId);
        IEnumerable<AccountCredentials> GetAllCredentials();
    }
}

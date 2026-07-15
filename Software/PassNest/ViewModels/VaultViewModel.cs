using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using PassNest.Models;
using System.Collections.ObjectModel;
using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.PasswordGeneration;
using System.Linq;
using EntityLayer;

namespace PassNest.ViewModels
{
    public partial class VaultViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly IPasswordGenerator passwordGenerator;
        private int? selectedCategoryId;

        private static readonly string[] AvatarPallete =
        {
            "#E15B4D", "#1B1F24", "#2563EB", "#1DB954", "#D97706",
            "#7C5CD6", "#0F7B8A", "#D6503C", "#2AA26A", "#1E3A5F"
        };

        [ObservableProperty]
        private string selectedCategoryFilter = "Sve";

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isAddAccountDialogOpen;

        [ObservableProperty]
        private NewAccountViewModel? newAccount;

        public event Action<AccountCardViewModel>? AccountOpened;

        public ObservableCollection<CategoryNavItem> Categories { get; } = new();

        public ObservableCollection<AccountCardViewModel> Accounts { get; } = new();

        public VaultViewModel(IAccountStore accountStore, IPasswordGenerator passwordGenerator)
        {
            this.accountStore = accountStore;
            this.passwordGenerator = passwordGenerator;

            LoadCategories();
            LoadAccounts();
        }

        private void LoadCategories()
        {
            Categories.Clear();

            var allCount = accountStore.GetAllAccounts().Count();
            Categories.Add(new CategoryNavItem(null, "Sve", allCount, "#1E8A91", isSelected: true));

            foreach(var category in accountStore.GetCategories())
            {
                var count = accountStore.FilterByCategory(category.CategoryId).Count();
                Categories.Add(new CategoryNavItem(category.CategoryId, category.Name, count, category.Color));
            }
        }

        private void LoadAccounts()
        {
            var accounts = selectedCategoryId is null ? accountStore.GetAllAccounts() : accountStore.FilterByCategory(selectedCategoryId.Value);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                accounts = accounts.Where(a => a.ServiceName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            var credentialsById = accountStore.GetAllCredentials().ToDictionary(c => c.AccountId);

            Accounts.Clear();
            foreach(var account in accounts)
            {
                if(!credentialsById.TryGetValue(account.AccountId, out var credentials))
                {
                    continue;
                }

                Accounts.Add(BuildCard(account, credentials.Password));
            }
        }

        private AccountCardViewModel BuildCard(Account account, string password)
        {
            var strength = passwordGenerator.EvaluateStrength(password);
            var categories = account.Categories.Select(c => new CategoryBadge(c.Name, c.Color)).ToList();

            return new AccountCardViewModel(
                account.AccountId,
                account.ServiceName.Length > 0 ? account.ServiceName[..1].ToUpper() : "?",
                account.ServiceName,
                account.UserName,
                GetAvatarColor(account.ServiceName),
                categories,
                strength,
                password,
                account.Url ?? string.Empty,
                account.UpdatedAt.ToString("dd.MM.yyyy"),
                account.CreatedAt.ToString("dd.MM.yyyy"));
        }

        private static string GetAvatarColor(string serviceName)
        {
            var index = Math.Abs(serviceName.GetHashCode()) % AvatarPallete.Length;
            return AvatarPallete[index];
        }

        partial void OnSearchTextChanged(string value) => LoadAccounts();

        [RelayCommand]
        private void SelectCategory(CategoryNavItem category)
        {
            foreach (var item in Categories)
                item.IsSelected = item == category;

            SelectedCategoryFilter = category.Name;
            selectedCategoryId = category.CategoryId;
            LoadAccounts();
        }

        [RelayCommand]
        private void AddAccount()
        {
            var dialog = new NewAccountViewModel(accountStore);
            dialog.Saved += () =>
            {
                LoadCategories();
                LoadAccounts();
            };

            dialog.Closed += () => IsAddAccountDialogOpen = false;
            NewAccount = dialog;
            IsAddAccountDialogOpen = true;
        }

        [RelayCommand]
        private void OpenAccountDetails(AccountCardViewModel account)
        {
            AccountOpened?.Invoke(account);
        }

        [RelayCommand]
        private void CopyPassword(AccountCardViewModel account)
        {
        }
    }
}

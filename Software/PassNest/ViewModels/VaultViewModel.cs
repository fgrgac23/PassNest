using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EntityLayer;
using PassNest.Models;
using PassNest.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class VaultViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IClipboardService clipboardService;
        private int? selectedCategoryId;
        private CancellationTokenSource? successDismissCts;

        [ObservableProperty]
        private string selectedCategoryFilter = "Sve";

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isAddAccountDialogOpen;

        [ObservableProperty]
        private NewAccountViewModel? newAccount;

        [ObservableProperty]
        private string? successMessage;

        [ObservableProperty]
        private bool hasSuccess;

        public event Action<AccountCardViewModel>? AccountOpened;

        public ObservableCollection<CategoryNavItem> Categories { get; } = new();

        public ObservableCollection<AccountCardViewModel> Accounts { get; } = new();

        public VaultViewModel(IAccountStore accountStore, IPasswordGenerator passwordGenerator, IClipboardService clipboardService)
        {
            this.accountStore = accountStore;
            this.passwordGenerator = passwordGenerator;
            this.clipboardService = clipboardService;

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
                AvatarColorPicker.GetInitial(account.ServiceName),
                account.ServiceName,
                account.UserName,
                AvatarColorPicker.GetColor(account.ServiceName),
                categories,
                strength,
                password,
                account.Url ?? string.Empty,
                account.UpdatedAt.ToString("dd.MM.yyyy"),
                account.CreatedAt.ToString("dd.MM.yyyy"));
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
            var dialog = new NewAccountViewModel(accountStore, passwordGenerator, clipboardService);
            dialog.Saved += () =>
            {
                LoadCategories();
                LoadAccounts();
            };

            dialog.Closed += () =>
            {
                IsAddAccountDialogOpen = false;
                LoadCategories();
            };

            NewAccount = dialog;
            IsAddAccountDialogOpen = true;
        }

        [RelayCommand]
        private void OpenAccountDetails(AccountCardViewModel account)
        {
            AccountOpened?.Invoke(account);
        }

        [RelayCommand]
        private async Task CopyPassword(AccountCardViewModel account)
        {
            await clipboardService.SetTextAsync(account.Password);
            ShowSuccess("Lozinka je uspješno kopirana.");
        }

        private async void ShowSuccess(string message)
        {
            SuccessMessage = message;
            HasSuccess = true;

            successDismissCts?.Cancel();
            var cts = new CancellationTokenSource();
            successDismissCts = cts;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                HasSuccess = false;
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}

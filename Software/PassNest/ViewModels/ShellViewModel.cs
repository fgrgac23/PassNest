using PassNest.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.PasswordGeneration;

namespace PassNest.ViewModels
{
    public partial class ShellViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly IPasswordGenerator passwordGenerator;

        [ObservableProperty]
        private string selectedNavItem = "Trezor";

        [ObservableProperty]
        private ViewModelBase currentPage;

        public VaultViewModel? CurrentVault => CurrentPage as VaultViewModel;

        public bool IsTrezorActive => SelectedNavItem == "Trezor";
        public bool IsGeneratorActive => SelectedNavItem == "Generator";
        public bool IsSafetyActive => SelectedNavItem == "Sigurnost";
        public bool IsSettingsActive => SelectedNavItem == "Postavke";

        public ShellViewModel(IAccountStore accountStore, IPasswordGenerator passwordGenerator)
        {
            this.accountStore = accountStore;
            this.passwordGenerator = passwordGenerator;
            currentPage = CreateVaultPage();
        }

        private VaultViewModel CreateVaultPage()
        {
            var page = new VaultViewModel(accountStore, passwordGenerator);
            page.AccountOpened += OnAccountOpened;
            return page;
        }

        private void OnAccountOpened(AccountCardViewModel account)
        {
            var detail = new AccountDetailViewModel(accountStore, account);
            detail.BackRequested += OnAccountDetailBackRequested;
            detail.Deleted += OnAccountDetailBackRequested;
            CurrentPage = detail;
        }

        private void OnAccountDetailBackRequested()
        {
            CurrentPage = CreateVaultPage();
        }

        partial void OnSelectedNavItemChanged(string value)
        {
            OnPropertyChanged(nameof(IsTrezorActive));
            OnPropertyChanged(nameof(IsGeneratorActive));
            OnPropertyChanged(nameof(IsSafetyActive));
            OnPropertyChanged(nameof(IsSettingsActive));

            CurrentPage = value switch
            {
                "Trezor" => CreateVaultPage(),
                "Generator" => new GeneratorViewModel(),
                "Sigurnost" => new SecurityViewModel(),
                "Postavke" => new SettingsViewModel(),
                _ => currentPage
            }; 
        }

        partial void OnCurrentPageChanged(ViewModelBase value)
        {
            OnPropertyChanged(nameof(CurrentVault));
        }

        [RelayCommand]
        private void SelectNav(string item) => SelectedNavItem = item;

        [RelayCommand]
        private void LockVault()
        {
        }
    }
}

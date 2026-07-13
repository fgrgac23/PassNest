using Avalonia.Automation.Peers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassNest.ViewModels
{
    public partial class ShellViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string selectedNavItem = "Trezor";

        [ObservableProperty]
        private ViewModelBase currentPage;

        public VaultViewModel? CurrentVault => CurrentPage as VaultViewModel;

        public bool IsTrezorActive => SelectedNavItem == "Trezor";
        public bool IsGeneratorActive => SelectedNavItem == "Generator";
        public bool IsSafetyActive => SelectedNavItem == "Sigurnost";
        public bool IsSettingsActive => SelectedNavItem == "Postavke";

        public ShellViewModel()
        {
            currentPage = CreateVaultPage();
        }

        private VaultViewModel CreateVaultPage()
        {
            var page = new VaultViewModel();
            page.AccountOpened += OnAccountOpened;
            return page;
        }

        private void OnAccountOpened(AccountCardViewModel account)
        {
            var detail = new AccountDetailViewModel(account);
            detail.BackRequested += OnAccountDetailBackRequested;
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

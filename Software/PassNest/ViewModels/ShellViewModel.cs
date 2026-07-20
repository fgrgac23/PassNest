using PassNest.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.PasswordGeneration;
using BusinessLogicLayer.Authentication;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace PassNest.ViewModels
{
    public partial class ShellViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IAuthProvider authProvider;

        [ObservableProperty]
        private string selectedNavItem = "Trezor";

        [ObservableProperty]
        private ViewModelBase currentPage;

        [ObservableProperty]
        private ObservableCollection<BreadcrumbItem> breadcrumbs = new();

        [ObservableProperty]
        private bool isDialogOpen;

        public VaultViewModel? CurrentVault => CurrentPage as VaultViewModel;

        public bool IsTrezorActive => SelectedNavItem == "Trezor";
        public bool IsGeneratorActive => SelectedNavItem == "Generator";
        public bool IsSafetyActive => SelectedNavItem == "Sigurnost";
        public bool IsSettingsActive => SelectedNavItem == "Postavke";

        public event Action? VaultLocked;

        public ShellViewModel(IAccountStore accountStore, IPasswordGenerator passwordGenerator, IAuthProvider authProvider)
        {
            this.accountStore = accountStore;
            this.passwordGenerator = passwordGenerator;
            this.authProvider = authProvider;
            currentPage = CreateVaultPage();
            UpdateBreadcrumbs();
        }

        private VaultViewModel CreateVaultPage()
        {
            var page = new VaultViewModel(accountStore, passwordGenerator);
            page.AccountOpened += OnAccountOpened;
            page.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(VaultViewModel.IsAddAccountDialogOpen)) IsDialogOpen = page.IsAddAccountDialogOpen;
            };

            IsDialogOpen = false;
            return page;
        }

        private void OnAccountOpened(AccountCardViewModel account)
        {
            var detail = new AccountDetailViewModel(accountStore, account);
            detail.BackRequested += OnAccountDetailBackRequested;
            detail.Deleted += OnAccountDetailBackRequested;

            detail.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(AccountDetailViewModel.IsEditing)) UpdateBreadcrumbs();
            };

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
            UpdateBreadcrumbs();
        }

        [RelayCommand]
        private void SelectNav(string item) => SelectedNavItem = item;

        [RelayCommand]
        private void LockVault()
        {
            authProvider.Logout();
            VaultLocked?.Invoke();
        }

        private void UpdateBreadcrumbs()
        {
            var items = new List<BreadcrumbItem>();

            switch (CurrentPage)
            {
                case AccountDetailViewModel detail:
                    items.Add(new BreadcrumbItem("Trezor", () => CurrentPage = CreateVaultPage()));
                    if (detail.IsEditing)
                    {
                        items.Add(new BreadcrumbItem("Detalji", () => detail.IsEditing = false));
                        items.Add(new BreadcrumbItem("Uredi"));
                    }
                    else
                    {
                        items.Add(new BreadcrumbItem("Detalji"));
                    }
                    break;

                case VaultViewModel:
                    items.Add(new BreadcrumbItem("Trezor"));
                    break;

                case GeneratorViewModel:
                    items.Add(new BreadcrumbItem("Generator"));
                    break;

                case SecurityViewModel:
                    items.Add(new BreadcrumbItem("Sigurnost"));
                    break;

                case SettingsViewModel:
                    items.Add(new BreadcrumbItem("Postavke"));
                    break;
            }

            for(var i = 1; i < items.Count; i++)
            {
                items[i].ShowSeparator = true;
            }

            Breadcrumbs = new ObservableCollection<BreadcrumbItem>(items);
        }
    }
}

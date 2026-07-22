using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Autofill;
using BusinessLogicLayer.BaseBackup;
using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.Models;
using PassNest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PassNest.ViewModels
{
    public partial class ShellViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IClipboardService clipboardService;
        private readonly IAuthProvider authProvider;
        private readonly IAutofillEngine autofillEngine;
        private readonly IBackupManager backupManager;
        private readonly IFIleDialogService fileDialogService;

        [ObservableProperty]
        private string selectedNavItem = "Trezor";

        [ObservableProperty]
        private ViewModelBase currentPage;

        [ObservableProperty]
        private ObservableCollection<BreadcrumbItem> breadcrumbs = new();

        [ObservableProperty]
        private bool isDialogOpen;

        public VaultViewModel? CurrentVault => CurrentPage as VaultViewModel;
        public AccountDetailViewModel? CurrentDetail => CurrentPage as AccountDetailViewModel;
        public GeneratorViewModel? CurrentGenerator => CurrentPage as GeneratorViewModel;
        public SettingsViewModel? CurrentSettings => CurrentPage as SettingsViewModel;

        public bool IsTrezorActive => SelectedNavItem == "Trezor";
        public bool IsGeneratorActive => SelectedNavItem == "Generator";
        public bool IsSafetyActive => SelectedNavItem == "Sigurnost";
        public bool IsSettingsActive => SelectedNavItem == "Postavke";

        public event Action? VaultLocked;

        public ShellViewModel(IAccountStore accountStore, IPasswordGenerator passwordGenerator, IClipboardService clipboardService, IAuthProvider authProvider, IAutofillEngine autofillEngine, IBackupManager backupManager, IFIleDialogService fileDialogService)
        {
            this.accountStore = accountStore;
            this.passwordGenerator = passwordGenerator;
            this.clipboardService = clipboardService;
            this.authProvider = authProvider;
            this.autofillEngine = autofillEngine;
            this.backupManager = backupManager;
            this.fileDialogService = fileDialogService;
            currentPage = CreateVaultPage();
            UpdateBreadcrumbs();
        }

        private VaultViewModel CreateVaultPage()
        {
            var page = new VaultViewModel(accountStore, passwordGenerator, clipboardService);
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
            var detail = new AccountDetailViewModel(accountStore, account, clipboardService, autofillEngine);
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
                "Generator" => new GeneratorViewModel(passwordGenerator, clipboardService),
                "Sigurnost" => new SecurityViewModel(),
                "Postavke" => new SettingsViewModel(backupManager, fileDialogService),
                _ => currentPage
            }; 
        }

        partial void OnCurrentPageChanged(ViewModelBase value)
        {
            OnPropertyChanged(nameof(CurrentVault));
            OnPropertyChanged(nameof(CurrentDetail));
            OnPropertyChanged(nameof(CurrentGenerator));
            OnPropertyChanged(nameof(CurrentSettings));
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

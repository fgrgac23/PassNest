using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Autofill;
using BusinessLogicLayer.BaseBackup;
using BusinessLogicLayer.PasswordAudit;
using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using PassNest.Services;
using System;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IAuthProvider authProvider;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IAccountStore accountStore;
        private readonly IClipboardService clipboardService;
        private readonly IAutofillEngine autofillEngine;
        private readonly IBackupManager backupManager;
        private readonly IFIleDialogService fIleDialogService;
        private readonly IIdleTimerService idleTimerService;
        private readonly IPasswordAuditor passwordAuditor;

        [ObservableProperty]
        private ViewModelBase currentPage;

        public event Action? ShowMainWindowRequested;

        public MainWindowViewModel(IAuthProvider authProvider, IPasswordGenerator passwordGenerator, IAccountStore accountStore, IClipboardService clipboardService, IAutofillEngine autofillEngine, IBackupManager backupManager, IFIleDialogService fIleDialogService, IIdleTimerService idleTimerService, IPasswordAuditor passwordAuditor)
        {
            this.authProvider = authProvider;
            this.passwordGenerator = passwordGenerator;
            this.accountStore = accountStore;
            this.clipboardService = clipboardService;
            this.autofillEngine = autofillEngine;
            this.autofillEngine.HotkeyPressed += OnHotKeyPressed;
            this.autofillEngine.RegisterHotkeys();
            this.backupManager = backupManager;
            this.fIleDialogService = fIleDialogService;
            this.idleTimerService = idleTimerService;
            this.passwordAuditor = passwordAuditor;
            CurrentPage = CreateInitialPage();
        }

        private void OnHotKeyPressed()
        {
            ShowMainWindowRequested?.Invoke();
        }

        private ViewModelBase CreateInitialPage() => authProvider.HasRegisteredUser() ? CreateLoginPage() : (ViewModelBase)CreateRegisterPage();

        private LoginViewModel CreateLoginPage()
        {
            var vm = new LoginViewModel(authProvider);
            vm.LoginSucceded += OnAuthenticated;
            vm.TwoFacotrRequired += OnTwoFactorRequired;
            return vm;
        }

        private void OnTwoFactorRequired()
        {
            var vm = new TwoFactorViewModel(authProvider);
            vm.VerificationSucceeded += OnAuthenticated;
            vm.BackRequested += () => CurrentPage = CreateLoginPage();
            CurrentPage = vm;
        }

        private RegisterViewModel CreateRegisterPage()
        {
            var vm = new RegisterViewModel(authProvider, passwordGenerator);
            vm.RegistrationSucceeded += OnAuthenticated;
            return vm;
        }

        private async void OnAuthenticated()
        {
            CurrentPage = new LoadingViewModel();
            await Task.Delay(1500);
            CurrentPage = CreateShellPage();
        }

        private ShellViewModel CreateShellPage()
        {
            var vm = new ShellViewModel(accountStore, passwordGenerator, clipboardService, authProvider, autofillEngine, backupManager, fIleDialogService, idleTimerService, passwordAuditor);
            vm.VaultLocked += OnVaultLocked;
            return vm;
        }

        private void OnVaultLocked()
        {
            CurrentPage = CreateLoginPage();
        }
    }
}

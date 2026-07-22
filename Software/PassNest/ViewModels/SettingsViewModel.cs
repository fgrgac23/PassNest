using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.BaseBackup;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly IBackupManager backupManager;
        private readonly IFIleDialogService fileDialogService;
        private readonly IAuthProvider authProvider;
        private bool isInitializing = true;
        private CancellationTokenSource? errorDismissCts;
        private CancellationTokenSource? successDismissCts;
        private string? pendingRestoreFilePath;

        [ObservableProperty]
        private bool twoFactorEnabled;

        [ObservableProperty]
        private string twoFactorEmail = string.Empty;

        public ObservableCollection<string> AutoLockOptions { get; } = new()
        {
            "1 minuta", "5 minuta", "15 minuta", "30 minuta", "Nikad"
        };

        [ObservableProperty]
        private string selectedAutoLockOptions = "5 minuta";

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string? successMessage;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool hasSuccess;

        [ObservableProperty]
        private bool isRestorePasswordPromptOpen;

        [ObservableProperty]
        private string restoreMasterPassword = string.Empty;


        public SettingsViewModel(IBackupManager backupManager, IFIleDialogService fileDialogService, IAuthProvider authProvider)
        {
            this.backupManager = backupManager;
            this.fileDialogService = fileDialogService;
            this.authProvider = authProvider;

            var user = authProvider.GetCurrentUser();
            if(user != null)
            {
                TwoFactorEnabled = user.Is2FAEnabled;
                TwoFactorEmail = MaskEmail(user.Email);
            }

            isInitializing = false;
        }

        partial void OnTwoFactorEnabledChanged(bool value)
        {
            if (isInitializing) return;

            var user = authProvider.GetCurrentUser();
            if (user == null) return;

            if (value)
            {
                authProvider.EnableTwoFactor(user.Email);
            }
            else
            {
                authProvider.DisableTwoFactor();
            }
        }

        private static string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 1) return email;

            var localPart = email[..atIndex];
            var domain = email[atIndex..];

            return $"{localPart[0]}{new string('*', Math.Max(localPart.Length - 1, 3))}{domain}";
        }

        [RelayCommand]
        private async Task ExportBackup()
        {
            var filePath = await fileDialogService.ChooseSaveLocationAsync($"passnest-backup-{DateTime.Now:yyyy-MM-dd}.json");
            if (string.IsNullOrWhiteSpace(filePath)) return;

            try
            {
                backupManager.CreateBackup(filePath);
                ShowSuccess("Izrada kopije uspješna.");
            }
            catch (Exception)
            {
                ShowError("Izrada kopije neuspješna.");
            }
        }

        [RelayCommand]
        private async Task RestoreBackup()
        {
            var filePath = await fileDialogService.ChooseOpenLocationAsync();
            if(string.IsNullOrWhiteSpace(filePath)) return;

            pendingRestoreFilePath = filePath;
            RestoreMasterPassword = string.Empty;
            IsRestorePasswordPromptOpen = true;
        }

        [RelayCommand]
        private void ConfirmRestore()
        {
            if (pendingRestoreFilePath == null) return;

            try
            {
                backupManager.RestoreBackup(pendingRestoreFilePath, RestoreMasterPassword);
                ShowSuccess("Vraćanje kopije uspješno.");
            }
            catch (Exception)
            {
                ShowError("Vraćanje kopije neuspješno — provjeri lozinku i datoteku.");
            }

            RestoreMasterPassword = string.Empty;
            pendingRestoreFilePath = null;
            IsRestorePasswordPromptOpen = false;
        }

        [RelayCommand]
        private void CancelRestorePrompt()
        {
            RestoreMasterPassword = string.Empty;
            pendingRestoreFilePath = null;
            IsRestorePasswordPromptOpen = false;
        }

        private async void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;

            errorDismissCts?.Cancel();
            var cts = new CancellationTokenSource();
            errorDismissCts = cts;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                HasError = false;
            }
            catch (TaskCanceledException)
            {
            }
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

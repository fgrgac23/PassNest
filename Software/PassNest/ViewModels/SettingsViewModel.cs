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
        private CancellationTokenSource? errorDismissCts;
        private CancellationTokenSource? successDismissCts;

        [ObservableProperty]
        private bool twoFactorEnabled = true;

        [ObservableProperty]
        private string twoFactorEmail = "i********@gmail.com";

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

        public SettingsViewModel(IBackupManager backupManager, IFIleDialogService fileDialogService)
        {
            this.backupManager = backupManager;
            this.fileDialogService = fileDialogService;
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

            try
            {
                backupManager.RestoreBackup(filePath);
                ShowSuccess("Vraćanje kopije uspješno.");
            }
            catch (Exception)
            {
                ShowError("Vraćanje kopije neuspješno.");
            }
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

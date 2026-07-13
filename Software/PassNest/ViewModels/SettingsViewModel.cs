using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassNest.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
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

        [RelayCommand]
        private void ExportBackup()
        {
        }

        [RelayCommand]
        private void RestoreBackup()
        {
        }
    }
}

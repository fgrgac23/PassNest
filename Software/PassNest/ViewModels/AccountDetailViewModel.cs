using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.ViewModels;

namespace PassNest.ViewModels
{
    public partial class AccountDetailViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string initial = "G";

        [ObservableProperty]
        private IBrush avatarColor = new SolidColorBrush(Color.Parse("#1B1F24"));

        [ObservableProperty]
        private string serviceName = "Github";

        [ObservableProperty]
        private string categoryName = "Posao";

        [ObservableProperty]
        private IBrush categoryColor = new SolidColorBrush(Color.Parse("#7C5CD6"));

        [ObservableProperty]
        private IBrush categoryTint = new SolidColorBrush(Color.Parse("#7C5CD6"), 0.15);

        [ObservableProperty]
        private string strengthLabel = "Jaka lozinka";

        [ObservableProperty]
        private IBrush strengthColor = new SolidColorBrush(Color.Parse("#2AA26A"));

        [ObservableProperty]
        private string username = "iivic";

        [ObservableProperty]
        private string password = "lozinka123";

        [ObservableProperty]
        private bool isPasswordRevealed;

        [ObservableProperty]
        private string url = "guthub.com/login";

        [ObservableProperty]
        private string lastModified = "12.06.2026.";

        [ObservableProperty]
        private string createdAt = "03.01.2025.";

        public string MaskedPassword => new string('*', Password?.Length ?? 0);

        partial void OnPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(MaskedPassword));
        }

        [RelayCommand]
        private void Edit()
        {
        }

        [RelayCommand]
        private void Delete()
        {
        }

        [RelayCommand]
        private void AutoFill()
        {
        }

        [RelayCommand]
        private void CopyUsername()
        {
        }

        [RelayCommand]
        private void CopyPassword()
        {
        }

        [RelayCommand]
        private void TogglePasswordReveal()
        {
        }
    }
}

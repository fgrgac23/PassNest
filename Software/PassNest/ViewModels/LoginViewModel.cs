using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassNest.ViewModels
{
    public partial class LoginVewModel : ViewModelBase
    {
        [ObservableProperty]
        private string masterPassword = string.Empty;

        [ObservableProperty]
        private bool isPasswordRevealed;

        [RelayCommand]
        private void TogglePasswordReveal()
        {
            IsPasswordRevealed = !IsPasswordRevealed;
        }

        [RelayCommand]
        private void Unlock()
        {
        }
    }
}

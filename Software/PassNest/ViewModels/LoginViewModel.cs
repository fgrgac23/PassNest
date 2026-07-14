using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusinessLogicLayer.Authentication;
using System;

namespace PassNest.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAtuhProvider authProvider;

        [ObservableProperty]
        private string masterPassword = string.Empty;

        [ObservableProperty]
        private bool isPasswordRevealed;

        [ObservableProperty]
        private string? errorMessage;

        public event Action? LoginSucceded;
        public event Action? TwoFacotrRequired;

        public LoginViewModel(IAtuhProvider authProvider)
        {
            this.authProvider = authProvider;
        }

        [RelayCommand]
        private void TogglePasswordReveal()
        {
            IsPasswordRevealed = !IsPasswordRevealed;
        }

        [RelayCommand]
        private void Unlock()
        {
            ErrorMessage = null;
            var resutl = authProvider.Login(MasterPassword);

            if (!resutl.Success)
            {
                ErrorMessage = resutl.ErrorMessage;
                return;
            }

            if (resutl.RequiresTwoFactor)
            {
                TwoFacotrRequired?.Invoke();
                return;
            }

            LoginSucceded?.Invoke();
        }
    }
}

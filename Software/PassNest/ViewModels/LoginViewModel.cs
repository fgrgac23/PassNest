using BusinessLogicLayer.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IAuthProvider authProvider;
        private CancellationTokenSource? errorDismissCts;

        [ObservableProperty]
        private string masterPassword = string.Empty;

        [ObservableProperty]
        private bool isPasswordRevealed;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool hasError;

        public event Action? LoginSucceded;
        public event Action? TwoFacotrRequired;

        public LoginViewModel(IAuthProvider authProvider)
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
            HasError = false;
            ErrorMessage = null;
            var resutl = authProvider.Login(MasterPassword);

            if (!resutl.Success)
            {
                ShowError(resutl.ErrorMessage);
                return;
            }

            if (resutl.RequiresTwoFactor)
            {
                TwoFacotrRequired?.Invoke();
                return;
            }

            LoginSucceded?.Invoke();
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
    }
}

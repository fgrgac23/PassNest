using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusinessLogicLayer.Authentication;
using System;

namespace PassNest.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        private readonly IAuthProvider authProvider;

        [ObservableProperty]
        private int currentStep = 1;

        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string masterPassword = string.Empty;

        [ObservableProperty]
        private string confirmMasterPassword = string.Empty;

        [ObservableProperty]
        private bool isPasswordRevealed;

        [ObservableProperty]
        private bool isConfirmPasswordRevealed;

        [ObservableProperty]
        private string? errorMessage;

        public bool IsStep1Visible => CurrentStep == 1;
        public bool IsStep2Visible => CurrentStep == 2;

        public event Action? RegistrationSucceeded;

        public RegisterViewModel(IAuthProvider authProvider)
        {
            this.authProvider = authProvider;
        }

        partial void OnCurrentStepChanged(int value)
        {
            OnPropertyChanged(nameof(IsStep1Visible));
            OnPropertyChanged(nameof(IsStep2Visible));
        }

        [RelayCommand]
        private void NextStep()
        {
            if(string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "Ime i prezime su obavezni.";
                return;
            }

            ErrorMessage = null;
            CurrentStep = 2;
        }

        [RelayCommand]
        private void PreviousStep()
        {
            CurrentStep = 1;
        }

        [RelayCommand]
        private void TogglePasswordReveal()
        {
            IsPasswordRevealed = !IsPasswordRevealed;
        }

        [RelayCommand]
        private void ToggleConfirmPasswordReveal()
        {
            IsConfirmPasswordRevealed = !IsConfirmPasswordRevealed;
        }

        [RelayCommand]
        private void CreateValut()
        {
            if(MasterPassword != ConfirmMasterPassword)
            {
                ErrorMessage = "Lozinke se ne podudaraju.";
                return;
            }

            var result = authProvider.RegisterUser(FirstName, LastName, MasterPassword);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            RegistrationSucceeded?.Invoke();
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.ViewModels;

namespace PassNest.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
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

        public bool IsStep1Visible => CurrentStep == 1;
        public bool IsStep2Visible => CurrentStep == 2;

        public string RevealButtonText => IsPasswordRevealed ? "Hide" : "Reveal";

        partial void OnCurrentStepChanged(int value)
        {
            OnPropertyChanged(nameof(IsStep1Visible));
            OnPropertyChanged(nameof(IsStep2Visible));
        }

        partial void OnIsPasswordRevealedChanged(bool value)
        {
            OnPropertyChanged(nameof(RevealButtonText));
        }

        [RelayCommand]
        private void NextStep()
        {
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
            // Implement registration logic here
        }
    }
}

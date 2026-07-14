using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusinessLogicLayer.Authentication;
using System;
using System.Text.RegularExpressions;
using Avalonia.Media;
using BusinessLogicLayer.PasswordGeneration;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly IBrush InactiveSegmentColor = new SolidColorBrush(Color.Parse("#D8DCE2"));

        private readonly IAuthProvider authProvider;
        private CancellationTokenSource? errorDismissCts;
        private readonly IPasswordGenerator passwordGenerator;

        [ObservableProperty]
        private int currentStep = 1;

        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private bool isEmailInvalid;

        [ObservableProperty]
        private string? emailErrorMessage;

        [ObservableProperty]
        private string masterPassword = string.Empty;

        [ObservableProperty]
        private string confirmMasterPassword = string.Empty;

        [ObservableProperty]
        private bool isPasswordRevealed;

        [ObservableProperty]
        private bool isConfirmPasswordRevealed;

        [ObservableProperty]
        private bool isConfirmPasswordInvalid;

        [ObservableProperty]
        private string? confirmPasswordErrorMessage;

        [ObservableProperty]
        private string strengthLabel = string.Empty;

        [ObservableProperty]
        private IBrush strengthColor = InactiveSegmentColor;

        [ObservableProperty]
        private IBrush segment1Color = InactiveSegmentColor;

        [ObservableProperty]
        private IBrush segment2Color = InactiveSegmentColor;

        [ObservableProperty]
        private IBrush segment3Color = InactiveSegmentColor;

        [ObservableProperty]
        private IBrush segment4Color = InactiveSegmentColor;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool hasError;

        public bool IsStep1Visible => CurrentStep == 1;
        public bool IsStep2Visible => CurrentStep == 2;

        public event Action? RegistrationSucceeded;

        public RegisterViewModel(IAuthProvider authProvider, IPasswordGenerator passwordGenerator)
        {
            this.authProvider = authProvider;
            this.passwordGenerator = passwordGenerator;
        }

        partial void OnCurrentStepChanged(int value)
        {
            OnPropertyChanged(nameof(IsStep1Visible));
            OnPropertyChanged(nameof(IsStep2Visible));
        }

        partial void OnEmailChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                IsEmailInvalid = false;
                EmailErrorMessage = null;
                return;
            }

            var isValid = EmailPattern.IsMatch(value);
            IsEmailInvalid = !isValid;
            EmailErrorMessage = isValid ? null : "Unesite ispravnu e-mail adresu.";
        }

        partial void OnMasterPasswordChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                StrengthLabel = string.Empty;
                ApplySegment(filledCount: 0, colorHex: "#D8DCE2");
                return;
            }

            var level = passwordGenerator.EvaluateStrength(value);
            var (label, colorHex, filledCount) = level switch
            {
                PasswordStrengthLevel.VrloSlaba => ("Vrlo slaba", "#D6503C", 1),
                PasswordStrengthLevel.Slaba => ("Slaba", "#E0952E", 2),
                PasswordStrengthLevel.Srednja => ("Srednja", "#D4A62E", 3),
                _ => ("Jaka", "#2AA26A", 4),
            };

            StrengthLabel = label;
            ApplySegment(filledCount, colorHex);
        }

        private void ApplySegment(int filledCount, string colorHex)
        {
            var activeColor = new SolidColorBrush(Color.Parse(colorHex));

            StrengthColor = activeColor;
            Segment1Color = filledCount >= 1 ? activeColor : InactiveSegmentColor;
            Segment2Color = filledCount >= 2 ? activeColor : InactiveSegmentColor;
            Segment3Color = filledCount >= 3 ? activeColor : InactiveSegmentColor;
            Segment4Color = filledCount >= 4 ? activeColor : InactiveSegmentColor;
        }

        [RelayCommand]
        private void NextStep()
        {
            HasError = false;

            if(string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Ime, prezime i e-mail su obavezni.");
                return;
            }

            if(string.IsNullOrWhiteSpace(email) || IsEmailInvalid)
            {
                IsEmailInvalid = true;
                EmailErrorMessage = "Unesite ispravnu e-mail adresu.";
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
                IsConfirmPasswordInvalid = true;
                ConfirmPasswordErrorMessage = "Lozinke se ne podudaraju.";
                return;
            }
            IsConfirmPasswordInvalid = false;
            ConfirmPasswordErrorMessage = null;

            var result = authProvider.RegisterUser(FirstName, LastName, Email, MasterPassword);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            RegistrationSucceeded?.Invoke();
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

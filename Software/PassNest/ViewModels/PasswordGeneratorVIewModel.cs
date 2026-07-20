using Avalonia.Media;
using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class PasswordGeneratorViewModel : ObservableObject
    {
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IClipboardService clipboardService;
        private CancellationTokenSource? successDismissCts;

        private bool isEnforcingMinimum;
        private bool upperCaseAutoEnabled;
        private bool lowerCaseAutoEnabled;
        private bool numbersAutoEnabled;
        private bool specialCharsAutoEnabled;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private int length = 14;

        [ObservableProperty]
        private bool useUpperCase = true;

        [ObservableProperty]
        private bool useLowerCase = true;

        [ObservableProperty]
        private bool useNumbers = true;

        [ObservableProperty]
        private bool useSpecialChars = true;

        [ObservableProperty]
        public string strengthLabel = string.Empty;

        [ObservableProperty]
        public IBrush strengthColor = new SolidColorBrush(Color.Parse("#D6503C"));

        [ObservableProperty]
        private string? successMessage;

        [ObservableProperty]
        private bool hasSuccess;

        public PasswordGeneratorViewModel(IPasswordGenerator passwordGenerator, IClipboardService clipboardService)
        {
            this.passwordGenerator = passwordGenerator;
            this.clipboardService = clipboardService;
            Generate();
        }

        partial void OnPasswordChanged(string value)
        {
            var (label, colorHex) = passwordGenerator.EvaluateStrength(value) switch
            {
                PasswordStrengthLevel.Jaka => ("Jaka lozinka", "#2AA26A"),
                PasswordStrengthLevel.Srednja => ("Srednja lozinka", "#D4A62E"),
                PasswordStrengthLevel.Slaba => ("Slaba lozinka", "#E0952E"),
                _ => ("Vrlo slaba lozinka", "#D6503C")
            };

            StrengthLabel = label;
            StrengthColor = new SolidColorBrush(Color.Parse(colorHex));
        }

        partial void OnUseUpperCaseChanged(bool value)
        {
            if (isEnforcingMinimum) return;

            if (!value)
            {
                if (!UseLowerCase && !UseNumbers && !UseSpecialChars)
                {
                    upperCaseAutoEnabled = true;
                    isEnforcingMinimum = true;
                    UseUpperCase = true;
                    isEnforcingMinimum = false;
                }
                return;
            }

            upperCaseAutoEnabled = false;
            TurnOffAutoEnabledCategory();
        }

        partial void OnUseLowerCaseChanged(bool value)
        {
            if (isEnforcingMinimum) return;

            if (!value)
            {
                if (!UseUpperCase && !UseNumbers && !UseSpecialChars)
                {
                    lowerCaseAutoEnabled = true;
                    isEnforcingMinimum = true;
                    UseLowerCase = true;
                    isEnforcingMinimum = false;
                }
                return;
            }

            lowerCaseAutoEnabled = false;
            TurnOffAutoEnabledCategory();
        }
        partial void OnUseNumbersChanged(bool value)
        {
            if (isEnforcingMinimum) return;

            if (!value)
            {
                if (!UseUpperCase && !UseLowerCase && !UseSpecialChars)
                {
                    numbersAutoEnabled = true;
                    isEnforcingMinimum = true;
                    UseNumbers = true;
                    isEnforcingMinimum = false;
                }
                return;
            }

            numbersAutoEnabled = false;
            TurnOffAutoEnabledCategory();
        }
        partial void OnUseSpecialCharsChanged(bool value)
        {
            if (isEnforcingMinimum) return;

            if (!value)
            {
                if (!UseUpperCase && !UseLowerCase && !UseNumbers)
                {
                    specialCharsAutoEnabled = true;
                    isEnforcingMinimum = true;
                    UseSpecialChars = true;
                    isEnforcingMinimum = false;
                }
                return;
            }

            specialCharsAutoEnabled = false;
            TurnOffAutoEnabledCategory();
        }

        private void TurnOffAutoEnabledCategory()
        {
            if (upperCaseAutoEnabled) {
                upperCaseAutoEnabled = false; UseUpperCase = false;
            }
            else if (lowerCaseAutoEnabled) {
                lowerCaseAutoEnabled = false; UseLowerCase = false;
            }
            else if (numbersAutoEnabled) {
                numbersAutoEnabled = false; UseNumbers = false;
            }
            else if (specialCharsAutoEnabled) {
                specialCharsAutoEnabled = false; UseSpecialChars = false;
            }
        }

        [RelayCommand]
        private void Generate()
        {
            var options = new PasswordOptions
            {
                Length = length,
                UseUppercase = UseUpperCase,
                UseLowercase = UseLowerCase,
                UseDigits = UseNumbers,
                UseSpecialChars = UseSpecialChars
            };

            Password = passwordGenerator.GeneratePassword(options);
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            await clipboardService.SetTextAsync(Password);
            ShowSuccess("Lozinka je uspješno kopirana.");
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

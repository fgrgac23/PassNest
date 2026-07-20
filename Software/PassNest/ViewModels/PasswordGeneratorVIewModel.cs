using Avalonia.Media;
using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.Services;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class PasswordGeneratorViewModel : ObservableObject
    {
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IClipboardService clipboardService;

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
        }
        
        partial void OnUseUpperCaseChanged(bool value) => EnsureAtLeastOneCategorySelected();
        partial void OnUseLowerCaseChanged(bool value) => EnsureAtLeastOneCategorySelected();
        partial void OnUseNumbersChanged(bool value) => EnsureAtLeastOneCategorySelected();
        partial void OnUseSpecialCharsChanged(bool value) => EnsureAtLeastOneCategorySelected();

        private void EnsureAtLeastOneCategorySelected()
        {
            if (!UseUpperCase && !UseLowerCase && !UseNumbers && !UseSpecialChars)
            {
                UseSpecialChars = true;
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
        }

    }
}

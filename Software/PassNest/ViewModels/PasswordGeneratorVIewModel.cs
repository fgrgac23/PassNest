using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace PassNest.ViewModels
{
    public partial class PasswordGeneratorVIewModel : ObservableObject
    {
        [ObservableProperty]
        private string password = "v8$Kp2#mLq7!Wd";

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

        public string StrengthLabel => Password.Length switch
        {
            >= 12 => "Jaka lozinka",
            >= 8 => "Srednja lozinka",
            _ => "Slaba lozinka"
        };

        public IBrush StrengthColor => Password.Length switch
        {
            >= 12 => new SolidColorBrush(Color.Parse("#2AA26A")),
            >= 8 => new SolidColorBrush(Color.Parse("#E0952E")),
            _ => new SolidColorBrush(Color.Parse("#D6503C"))
        };

        partial void OnPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(StrengthLabel));
            OnPropertyChanged(nameof(StrengthColor));
        }

        [RelayCommand]
        private void Generate()
        {
        }

        [RelayCommand]
        private void CopyPassword()
        {
        }

    }
}

using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassNest.ViewModels
{
    public partial class NewAccountViewModel : ObservableObject
    {
        [ObservableProperty]
        private string serviceName = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = "v8$Kp2#mLq7!Wd";

        [ObservableProperty]
        private bool isGeneratorOpen;

        public double GeneratorPanelWidth => IsGeneratorOpen ? 857 : 504;

        partial void OnIsGeneratorOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(GeneratorPanelWidth));
        }

        public ObservableCollection<CategoryOption> Categories { get; } = new()
        {
            new CategoryOption("Financije", "#E0952E"),
            new CategoryOption("Posao", "#7C5CD6"),
            new CategoryOption("Zabava", "#D6503C"),
            new CategoryOption("Osobno", "#2AA26A"),
        };

        [ObservableProperty]
        private CategoryOption selectedCategory;

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

        public event Action? Closed;

        public NewAccountViewModel()
        {
            selectedCategory = Categories[1];
        }

        partial void OnPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(StrengthLabel));
            OnPropertyChanged(nameof(StrengthColor));
        }

        [RelayCommand]
        private void ToggleGenerator()
        {
            IsGeneratorOpen = !IsGeneratorOpen;
        }

        [RelayCommand]
        private void Generate()
        {
        }

        [RelayCommand]
        private void CopyPassword()
        {
        }

        [RelayCommand]
        private void Save()
        {
            Closed?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            Closed?.Invoke();
        }
    }

    public class CategoryOption
    {
        public string Name { get; }
        public IBrush DotColor { get; }

        public CategoryOption(string name, string colorHex)
        {
            Name = name;
            DotColor = new SolidColorBrush(Color.Parse(colorHex));
        }
    }
}

using System;
using System.Collections.ObjectModel;
using PassNest.Models;
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
        private PasswordGeneratorViewModel generator = new();

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

        public event Action? Closed;

        public NewAccountViewModel()
        {
            selectedCategory = Categories[1];
        }

        [RelayCommand]
        private void ToggleGenerator()
        {
            IsGeneratorOpen = !IsGeneratorOpen;
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
}

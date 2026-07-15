using System;
using System.Collections.ObjectModel;
using PassNest.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusinessLogicLayer.AccountManagement;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class NewAccountViewModel : ObservableObject
    {
        private readonly IAccountStore accountStore;
        private CancellationTokenSource? errorDismissCts;

        [ObservableProperty]
        private string serviceName = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private PasswordGeneratorViewModel generator = new();

        [ObservableProperty]
        private bool isGeneratorOpen;

        [ObservableProperty]
        private string?  errorMessage;

        [ObservableProperty]
        private bool hasError;

        public double GeneratorPanelWidth => IsGeneratorOpen ? 857 : 504;

        partial void OnIsGeneratorOpenChanged(bool value)
        {
            OnPropertyChanged(nameof(GeneratorPanelWidth));
        }

        public ObservableCollection<CategoryOption> Categories { get; }

        public event Action? Closed;
        public event Action? Saved;

        public NewAccountViewModel(IAccountStore accountStore)
        {
            this.accountStore = accountStore;

            Categories = new ObservableCollection<CategoryOption>(accountStore.GetCategories().Select(c => new CategoryOption(c.CategoryId, c.Name, c.Color)));
        }

        [RelayCommand]
        private void ToggleGenerator()
        {
            IsGeneratorOpen = !IsGeneratorOpen;
        }

        [RelayCommand]
        private void Save()
        {
            if(string.IsNullOrWhiteSpace(ServiceName) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Generator.Password))
            {
                ShowError("Naziv servisa, korisničko ime i lozinka su obavezni!");
                return;
            }

            var categoryIds = Categories.Where(c => c.IsSelected).Select(c => c.CategoryId);

            accountStore.AddAccount(ServiceName, Username, Generator.Password, categoryIds);

            Saved?.Invoke();
            Closed?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            Closed?.Invoke();
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

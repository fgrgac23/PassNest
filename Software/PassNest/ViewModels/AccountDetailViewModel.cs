using Avalonia.Media;
using BusinessLogicLayer.AccountManagement;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class AccountDetailViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly int accountId;
        private CancellationTokenSource? errorDismissCts;

        [ObservableProperty]
        private string initial;

        [ObservableProperty]
        private IBrush avatarColor;

        [ObservableProperty]
        private string serviceName;

        [ObservableProperty]
        public IReadOnlyList<CategoryBadge> categories;

        [ObservableProperty]
        private string strengthLabel;

        [ObservableProperty]
        private IBrush strengthColor;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isPasswordRevealed;

        [ObservableProperty]
        private string url;

        [ObservableProperty]
        private string lastModified;

        [ObservableProperty]
        private string createdAt;

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private string editServiceName = string.Empty;

        [ObservableProperty]
        private string editUsername = string.Empty;

        [ObservableProperty]
        private string editPassword = string.Empty;

        [ObservableProperty]
        private string editUrl = string.Empty;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool hasError;

        public ObservableCollection<CategoryOption> EditCategories { get; } = new();

        public event Action? BackRequested;
        public event Action? Deleted;

        public AccountDetailViewModel(IAccountStore accountStore, AccountCardViewModel account)
        {
            this.accountStore = accountStore;
            accountId = account.AccountId;
            initial = account.Initial;
            avatarColor = account.AvatarColor;
            serviceName = account.ServiceName;
            Categories = account.Categories;
            strengthLabel = account.StrengthLabel;
            strengthColor = account.StatusColor;
            username = account.Username;
            password = account.Password;
            url = account.Url;
            lastModified = account.LastModified;
            createdAt = account.CreatedAt;
        }

        public string MaskedPassword => new string('*', Password?.Length ?? 0);

        partial void OnPasswordChanged(string value)
        {
            OnPropertyChanged(nameof(MaskedPassword));
        }

        [RelayCommand]
        private void Edit()
        {
            EditServiceName = ServiceName;
            EditUsername = Username;
            EditPassword = Password;
            EditUrl = Url;

            var selectedNames = Categories.Select(c => c.Name).ToHashSet();

            EditCategories.Clear();
            foreach (var category in accountStore.GetCategories())
            {
                EditCategories.Add(new CategoryOption(category.CategoryId, category.Name, category.Color, selectedNames.Contains(category.Name)));
            }

            HasError = false;
            IsEditing = true;
        }

        [RelayCommand]
        private void SaveEdit()
        {
            if(string.IsNullOrWhiteSpace(EditServiceName) || string.IsNullOrWhiteSpace(EditUsername) || string.IsNullOrWhiteSpace(EditPassword))
            {
                ShowError("Naziv servisa, korisničko ime i lozinka su obavezni!");
                return;
            }

            var categoryIds = EditCategories.Where(c => c.IsSelected).Select(c => c.CategoryId);
            var newUrl = string.IsNullOrWhiteSpace(EditUrl) ? null : EditUrl;

            accountStore.UpdateAccount(accountId, EditServiceName, EditUsername, EditPassword, newUrl, categoryIds);

            ServiceName = EditServiceName;
            Username = EditUsername;
            Password = EditPassword;
            Url = EditUrl;
            Categories = EditCategories.Where(c => c.IsSelected).Select(c => new CategoryBadge(c.Name, c.ColorHex)).ToList();

            IsEditing = false;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditing = false;
        }

        [RelayCommand]
        private void Delete()
        {
            accountStore.DeleteAccount(accountId);
            Deleted?.Invoke();
        }

        [RelayCommand]
        private void AutoFill()
        {
        }

        [RelayCommand]
        private void CopyUsername()
        {
        }

        [RelayCommand]
        private void CopyPassword()
        {
        }

        [RelayCommand]
        private void TogglePasswordReveal()
        {
            IsPasswordRevealed = !IsPasswordRevealed;
        }

        [RelayCommand]
        private void GoBack()
        {
            BackRequested?.Invoke();
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

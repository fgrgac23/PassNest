using Avalonia.Media;
using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.Autofill;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassNest.Models;
using PassNest.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PassNest.ViewModels
{
    public partial class AccountDetailViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly IClipboardService clipboardService;
        private readonly int accountId;
        private CancellationTokenSource? errorDismissCts;
        private CancellationTokenSource? successDismissCts;
        private readonly IAutofillEngine autofillEngine;

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
        private string? successMessage;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool hasSuccess;

        public ObservableCollection<CategoryOption> EditCategories { get; } = new();

        public event Action? BackRequested;
        public event Action? Deleted;

        public AccountDetailViewModel(IAccountStore accountStore, AccountCardViewModel account, IClipboardService clipboardService, IAutofillEngine autofillEngine)
        {
            this.accountStore = accountStore;
            this.clipboardService = clipboardService;
            this.autofillEngine = autofillEngine;
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

        public bool HasUrl => !string.IsNullOrWhiteSpace(Url);

        partial void OnUrlChanged(string value)
        {
            OnPropertyChanged(nameof(HasUrl));
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
                EditCategories.Add(new CategoryOption(category.CategoryId, category.Name, category.Color, category.IsSystemDefined, selectedNames.Contains(category.Name)));
            }

            HasError = false;
            IsEditing = true;
        }

        [RelayCommand]
        private void DeleteCategory(CategoryOption category)
        {
            if (category.IsSystemDefined) return;

            accountStore.DeleteCategory(category.CategoryId);
            EditCategories.Remove(category);
        }

        [RelayCommand]
        private void SaveEdit()
        {
            if (string.IsNullOrWhiteSpace(EditServiceName) || string.IsNullOrWhiteSpace(EditUsername) || string.IsNullOrWhiteSpace(EditPassword))
            {
                ShowError("Naziv servisa, korisničko ime i lozinka su obavezni!");
                return;
            }

            var categoryIds = EditCategories.Where(c => c.IsSelected).Select(c => c.CategoryId);
            var newUrl = string.IsNullOrWhiteSpace(EditUrl) ? null : EditUrl;

            accountStore.UpdateAccount(accountId, EditServiceName, EditUsername, EditPassword, newUrl, categoryIds);

            Initial = AvatarColorPicker.GetInitial(EditServiceName);
            AvatarColor = new SolidColorBrush(Color.Parse(AvatarColorPicker.GetColor(EditServiceName)));
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
            var success = autofillEngine.TriggerAutofill(accountId);

            if (success)
            {
                ShowSuccess("Podaci su uspješno popunjeni.");
            }
            else
            {
                ShowError("Račun nije pronađen.");
            }
        }

        [RelayCommand]
        private void OpenUrl()
        {
            if (string.IsNullOrWhiteSpace(Url)) return;

            var target = Url.Contains("://") ? Url : $"https://{Url}";
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        }

        [RelayCommand]
        private async Task CopyUsername()
        {
            await clipboardService.SetTextAsync(Username);
            ShowSuccess("Korisničko ime je uspješno kopirano.");
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            await clipboardService.SetTextAsync(Password);
            ShowSuccess("Lozinka je uspješno kopirana.");
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

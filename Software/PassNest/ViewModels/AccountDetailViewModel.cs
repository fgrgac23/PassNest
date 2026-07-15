using Avalonia.Media;
using System;
using PassNest.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BusinessLogicLayer.AccountManagement;
using System.Collections.Generic;

namespace PassNest.ViewModels
{
    public partial class AccountDetailViewModel : ViewModelBase
    {
        private readonly IAccountStore accountStore;
        private readonly int accountId;

        [ObservableProperty]
        private string initial;

        [ObservableProperty]
        private IBrush avatarColor;

        [ObservableProperty]
        private string serviceName;

        public IReadOnlyList<CategoryBadge> Categories { get; }

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
    }
}

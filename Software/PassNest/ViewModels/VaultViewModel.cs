using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PassNest.ViewModels
{
    public partial class VaultViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string selectedNavItem = "Trezor";

        [ObservableProperty]
        private string selectedCategoryFilter = "Sve";

        [ObservableProperty]
        private string searchText = string.Empty;

        public bool IsTrezorActive => SelectedNavItem == "Trezor";
        public bool IsGeneratorActive => SelectedNavItem == "Generator";
        public bool IsSafetyActive => SelectedNavItem == "Sigurnost";
        public bool IsSettingsActive => SelectedNavItem == "Postavke";

        public ObservableCollection<CategoryNavItem> Categories { get; } = new()
        {
            new CategoryNavItem("Sve", 24, "#1E8A91", isSelected:true),
            new CategoryNavItem("Financije", 5, "#E0952E"),
            new CategoryNavItem("Posao", 7, "#7C5CD6"),
            new CategoryNavItem("Zabava", 8, "#D6503C"),
            new CategoryNavItem("Osobno", 4, "#2AA26A")
        };

        public ObservableCollection<AccountCardViewModel> Accounts { get; } = new()
        {
            new AccountCardViewModel("G", "Gmail", "ivan.ivic@gmail.com", "#E15B4D", "Osobno", "#2AA26A", "Strong"),
            new("G", "GitHub", "iivic", "#1B1F24", "Posao", "#7C5CD6", "Strong"),
            new("N", "Netflix", "ivan.ivic@gmail.com", "#D6503C", "Zabava", "#D6503C", "Medium"),
            new("P", "PayPal", "ivan.ivic@gmail.com", "#2563EB", "Financije", "#E0952E", "Strong"),
            new("S", "Spotify", "ivan.ivic", "#1DB954", "Zabava", "#D6503C", "Weak"),
            new("A", "Amazon", "ivan.ivic@gmail.com", "#D97706", "Financije", "#E0952E", "Strong"),
            new("S", "Steam", "ivan.ivic_hr", "#1E3A5F", "Zabava", "#D6503C", "Strong"),
            new("M", "Microsoft 365", "ivan.ivic@firma.hr", "#2F6FED", "Posao", "#7C5CD6", "Strong"),
            new("L", "LinkedIn", "ivan.ivic@gmail.com", "#0F7B8A", "Posao", "#7C5CD6", "Medium"),
        };

        partial void OnSelectedNavItemChanged(string value)
        {
            OnPropertyChanged(nameof(IsTrezorActive));
            OnPropertyChanged(nameof(IsGeneratorActive));
            OnPropertyChanged(nameof(IsSafetyActive));
            OnPropertyChanged(nameof(IsSettingsActive));
        }

        [RelayCommand]
        private void SelectNav(string item) => SelectedNavItem = item;

        [RelayCommand]
        private void SelectCategory(CategoryNavItem category)
        {
            foreach (var item in Categories)
                item.IsSelected = item == category;

            SelectedCategoryFilter = category.Name;
        }

        [RelayCommand]
        private void AddAccount()
        {
        }

        [RelayCommand]
        private void LockVault()
        {
        }

        [RelayCommand]
        private void OpenAccountDetails(AccountCardViewModel account)
        {
        }
    }
}

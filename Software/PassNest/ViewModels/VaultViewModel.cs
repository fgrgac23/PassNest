using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace PassNest.ViewModels
{
    public partial class VaultViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string selectedCategoryFilter = "Sve";

        [ObservableProperty]
        private string searchText = string.Empty;

        public event Action<AccountCardViewModel>? AccountOpened;

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
            new AccountCardViewModel("G", "Gmail", "ivan.ivic@gmail.com", "#E15B4D", "Osobno", "#2AA26A", "Strong", "gmail#Pass2025!", "mail.google.com", "10.06.2026.", "12.02.2024."),
            new("G", "GitHub", "iivic", "#1B1F24", "Posao", "#7C5CD6", "Strong", "GitHub$ecure99", "github.com/login", "01.06.2026.", "03.01.2025."),
            new("N", "Netflix", "ivan.ivic@gmail.com", "#D6503C", "Zabava", "#D6503C", "Medium", "netflix2024", "netflix.com/login", "20.05.2026.", "15.03.2023."),
            new("P", "PayPal", "ivan.ivic@gmail.com", "#2563EB", "Financije", "#E0952E", "Strong", "PayP@l!Secure77", "paypal.com/signin", "28.06.2026.", "07.11.2022."),
            new("S", "Spotify", "ivan.ivic", "#1DB954", "Zabava", "#D6503C", "Weak", "spotify1", "spotify.com/login", "14.04.2026.", "22.09.2023."),
            new("A", "Amazon", "ivan.ivic@gmail.com", "#D97706", "Financije", "#E0952E", "Strong", "Amaz0n#Buy2025", "amazon.com/signin", "25.06.2026.", "18.06.2022."),
            new("S", "Steam", "ivan.ivic_hr", "#1E3A5F", "Zabava", "#D6503C", "Strong", "SteamG@mer24", "steamcommunity.com/login", "12.06.2026.", "05.08.2021."),
            new("M", "Microsoft 365", "ivan.ivic@firma.hr", "#2F6FED", "Posao", "#7C5CD6", "Strong", "MS365#Work2026", "login.microsoftonline.com", "30.06.2026.", "10.01.2024."),
            new("L", "LinkedIn", "ivan.ivic@gmail.com", "#0F7B8A", "Posao", "#7C5CD6", "Medium", "linkedin2025", "linkedin.com/login", "05.06.2026.", "14.07.2023."),
        };

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
        private void OpenAccountDetails(AccountCardViewModel account)
        {
            AccountOpened?.Invoke(account);
        }

        [RelayCommand]
        private void CopyPassword(AccountCardViewModel account)
        {
        }
    }
}

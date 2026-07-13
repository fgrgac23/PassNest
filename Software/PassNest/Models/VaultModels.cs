using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PassNest.Models
{
    public class AccountCardViewModel
    {
        public string Initial { get; }
        public string ServiceName { get; }
        public string Username { get; }
        public IBrush AvatarColor { get; }
        public string CategoryName { get; }
        public IBrush CategoryColor { get; }
        public IBrush CategoryTint { get; }
        public IBrush StatusColor { get; }
        public string StrengthLabel { get; }
        public string Password { get; }
        public string Url { get; }
        public string LastModified { get; }
        public string CreatedAt { get; }

        public AccountCardViewModel(string initial, string serviceName, string username, string avatarColorHex, string categoryName, string categoryColorHex, string strength, string password, string url, string lastModified, string createdAt)
        {
            Initial = initial;
            ServiceName = serviceName;
            Username = username;
            AvatarColor = new SolidColorBrush(Color.Parse(avatarColorHex));
            CategoryName = categoryName;
            CategoryColor = new SolidColorBrush(Color.Parse(categoryColorHex));
            CategoryTint = new SolidColorBrush(Color.Parse(categoryColorHex), 0.15);
            StatusColor = new SolidColorBrush(Color.Parse(strength switch
            {
                "Strong" => "#2AA26A",
                "Medium" => "#E0952E",
                _ => "#D6503C"
            }));
            StrengthLabel = strength switch
            {
                "Strong" => "Jaka lozinka",
                "Medium" => "Srednja lozinka",
                _ => "Slaba lozinka"
            };
            Password = password;
            Url = url;
            LastModified = lastModified;
            CreatedAt = createdAt;
        }
    }

    public partial class CategoryNavItem : ObservableObject
    {
        public string Name { get; }
        public int Count { get; }
        public IBrush DotColor { get; }

        [ObservableProperty]
        private bool isSelected;
        public CategoryNavItem(string name, int count, string dotColorHex, bool isSelected = false)
        {
            Name = name;
            Count = count;
            DotColor = new SolidColorBrush(Color.Parse(dotColorHex));
            this.isSelected = isSelected;
        }
    }
}

using Avalonia.Media;
using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace PassNest.Models
{
    public class CategoryBadge
    {
        public string Name { get; }
        public IBrush Foreground { get; }
        public IBrush Tint { get; }

        public CategoryBadge(string name, string colorHex)
        {
            Name = name;
            Foreground = new SolidColorBrush(Color.Parse(colorHex));
            Tint = new SolidColorBrush(Color.Parse(colorHex), 0.15);
        }
    }
    public class AccountCardViewModel
    {
        public int AccountId { get; }
        public string Initial { get; }
        public string ServiceName { get; }
        public string Username { get; }
        public IBrush AvatarColor { get; }
        public IReadOnlyList<CategoryBadge> Categories { get; }
        public IBrush StatusColor { get; }
        public string StrengthLabel { get; }
        public string Password { get; }
        public string Url { get; }
        public string LastModified { get; }
        public string CreatedAt { get; }

        public AccountCardViewModel(int accountId, string initial, string serviceName, string username, string avatarColorHex, IReadOnlyList<CategoryBadge> categories, PasswordStrengthLevel strength, string password, string url, string lastModified, string createdAt)
        {
            AccountId = accountId;
            Initial = initial;
            ServiceName = serviceName;
            Username = username;
            AvatarColor = new SolidColorBrush(Color.Parse(avatarColorHex));
            Categories = categories;

            var (label, statusHex) = strength switch
            {
                PasswordStrengthLevel.Jaka => ("Jaka lozinka", "#2AA26A"),
                PasswordStrengthLevel.Srednja => ("Srednja lozinka", "#D4A62E"),
                PasswordStrengthLevel.Slaba => ("Slaba lozinka", "#E0952E"),
                _ => ("Vrlo slaba lozinka", "#D6503C")
            };
            StatusColor = new SolidColorBrush(Color.Parse(statusHex));
            StrengthLabel = label;
            Password = password;
            Url = url;
            LastModified = lastModified;
            CreatedAt = createdAt;
        }
    }

    public partial class CategoryNavItem : ObservableObject
    {
        public int? CategoryId { get; }
        public string Name { get; }
        public int Count { get; }
        public IBrush DotColor { get; }

        [ObservableProperty]
        private bool isSelected;
        public CategoryNavItem(int? categoryId, string name, int count, string dotColorHex, bool isSelected = false)
        {
            CategoryId = categoryId;
            Name = name;
            Count = count;
            DotColor = new SolidColorBrush(Color.Parse(dotColorHex));
            this.isSelected = isSelected;
        }
    }
}

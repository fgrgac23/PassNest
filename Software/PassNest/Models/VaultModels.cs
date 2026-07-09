using Avalonia.Media;

namespace PassNest.ViewModels
{
    public class AccountCardViewModel
    {
        public string Inital { get; }
        public string ServiceName { get; }
        public string Username { get; }
        public IBrush AvatarColor { get; }
        public string CategoryName { get; }
        public IBrush CategoryColor { get; }
        public IBrush CategoryTint { get; }
        public IBrush StatusColor { get; }

        public AccountCardViewModel(string inital, string serviceName, string username, string avatarColorHex, string categoryName, string categoryColorHex, string strength)
        {
            Inital = inital;
            ServiceName = serviceName;
            Username = username;
            AvatarColor = new SolidColorBrush(Color.Parse(avatarColorHex));
            CategoryName = categoryName;
            CategoryColor = new SolidColorBrush(Color.Parse(categoryColorHex));
            CategoryTint = new SolidColorBrush(Color.Parse(categoryColorHex),0.15);
            StatusColor = new SolidColorBrush(Color.Parse(strength switch
            {
                "Strong" => "#2AA26A",
                "Medium" => "#E0952E",
                _ => "#D6503C"
            }));
        }

        public class CategoryNavItem
        {
            public string Name { get; }
            public int Count { get; }
            public IBrush DotColor { get; }

            public CategoryNavItem(string name, int count, string dotColorHex)
            {
                Name = name;
                Count = count;
                DotColor = new SolidColorBrush(Color.Parse(dotColorHex));
            }
        }

    }
}

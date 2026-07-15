using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Models
{
    public partial class CategoryOption : ObservableObject
    {
        public int CategoryId { get; }
        public string Name { get; }
        public IBrush DotColor { get; }

        [ObservableProperty]
        private bool isSelected;

        public CategoryOption(int categoryId, string name, string colorHex, bool isSelected = false)
        {
            CategoryId = categoryId;
            Name = name;
            DotColor = new SolidColorBrush(Color.Parse(colorHex));
            this.isSelected = isSelected;
        }
    }
}

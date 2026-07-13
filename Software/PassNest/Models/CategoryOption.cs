using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Models
{
    public class CategoryOption
    {
        public string Name { get; }
        public IBrush DotColor { get; }

        public CategoryOption(string name, string colorHex)
        {
            Name = name;
            DotColor = new SolidColorBrush(Color.Parse(colorHex));
        }
    }
}

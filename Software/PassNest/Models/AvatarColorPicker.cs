using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Models
{
    public static class AvatarColorPicker
    {
        private static readonly string[] Pallete =
        {
            "#E15B4D", "#1B1F24", "#2563EB", "#1DB954", "#D97706",
            "#7C5CD6", "#0F7B8A", "#D6503C", "#2AA26A", "#1E3A5F"
        };

        public static string GetColor(string serviceName)
        {
            var index = Math.Abs(serviceName.GetHashCode()) % Pallete.Length;
            return Pallete[index];
        }

        public static string GetInitial(string serviceName) => serviceName.Length > 0 ? serviceName[..1].ToUpper() : "?";
    }
}

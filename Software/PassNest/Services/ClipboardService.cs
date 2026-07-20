using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Services
{
    public class ClipboardService : IClipboardService
    {
        public async Task SetTextAsync(string text)
        {
            var clipboard = GetClipboard();
            if (clipboard == null) return;

            await clipboard.SetTextAsync(text);
        }

        private static IClipboard? GetClipboard()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) return desktop.MainWindow?.Clipboard;

            return null;
        }
    }
}

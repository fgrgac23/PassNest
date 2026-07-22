using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Services
{
    public class FileDialogService : IFIleDialogService
    {
        public async Task<string?> ChooseOpenLocationAsync()
        {
            var storageProvider = GetStorageProvider();
            if (storageProvider == null) return null;

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Odaberi sigurnosnu kopiju",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("PassNest sigurnosna kopija") { Patterns = new[] { "*.json" } } }
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }

        public async Task<string?> ChooseSaveLocationAsync(string suggestedFileName)
        {
            var storageProvider = GetStorageProvider();
            if (storageProvider == null) return null;

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Spremi sigurnosnu kopiju",
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = new[] { new FilePickerFileType("PassNest sigurnosna kopija") { Patterns = new[] { "*.json" } } }
            });

            return file?.Path.LocalPath;
        }

        private static IStorageProvider? GetStorageProvider()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) return desktop.MainWindow?.StorageProvider;

            return null;
        }
    }
}

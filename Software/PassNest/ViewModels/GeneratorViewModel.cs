using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using PassNest.Services;

namespace PassNest.ViewModels
{
    public partial class GeneratorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private PasswordGeneratorViewModel generator;

        public GeneratorViewModel(IPasswordGenerator passwordGenerator, IClipboardService clipboardService)
        {
            generator = new PasswordGeneratorViewModel(passwordGenerator, clipboardService);
        }
    }
}

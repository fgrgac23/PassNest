using CommunityToolkit.Mvvm.ComponentModel;

namespace PassNest.ViewModels
{
    public partial class GeneratorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private PasswordGeneratorViewModel generator = new()
        {
            Password = "v8$Kp2#mLq7!Wd&9r",
            Length = 18
        };
    }
}

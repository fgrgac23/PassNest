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

        public int CharacterCount => Generator.Password.Length;

        public GeneratorViewModel()
        {
            Generator.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PasswordGeneratorViewModel.Password))
                    OnPropertyChanged(nameof(CharacterCount));
            };
        }
    }
}

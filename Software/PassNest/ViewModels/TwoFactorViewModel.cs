using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassNest.ViewModels
{
    public partial class TwoFactorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string maskedEmail = "i****@gmail.com";

        [ObservableProperty]
        private string digit1 = string.Empty;

        [ObservableProperty]
        private string digit2 = string.Empty;

        [ObservableProperty]
        private string digit3 = string.Empty;

        [ObservableProperty]
        private string digit4 = string.Empty;

        [ObservableProperty]
        private string digit5 = string.Empty;

        [ObservableProperty]
        private string digit6 = string.Empty;

        [ObservableProperty]
        private string resendCountdown = "0:42";

        public bool IsDigit1Filled => !string.IsNullOrEmpty(Digit1);
        public bool IsDigit2Filled => !string.IsNullOrEmpty(Digit2);
        public bool IsDigit3Filled => !string.IsNullOrEmpty(Digit3);
        public bool IsDigit4Filled => !string.IsNullOrEmpty(Digit4);
        public bool IsDigit5Filled => !string.IsNullOrEmpty(Digit5);
        public bool IsDigit6Filled => !string.IsNullOrEmpty(Digit6);

        partial void OnDigit1Changed(string value) => OnPropertyChanged(nameof(IsDigit1Filled));
        partial void OnDigit2Changed(string value) => OnPropertyChanged(nameof(IsDigit2Filled));
        partial void OnDigit3Changed(string value) => OnPropertyChanged(nameof(IsDigit3Filled));
        partial void OnDigit4Changed(string value) => OnPropertyChanged(nameof(IsDigit4Filled));
        partial void OnDigit5Changed(string value) => OnPropertyChanged(nameof(IsDigit5Filled));
        partial void OnDigit6Changed(string value) => OnPropertyChanged(nameof(IsDigit6Filled));


        [RelayCommand]
        private void ConfirmCode()
        {
        }

        [RelayCommand]
        private void ResendCode()
        {
        }

    }
}

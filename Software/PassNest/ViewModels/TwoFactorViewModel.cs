using Avalonia.Media;
using Avalonia.Threading;
using BusinessLogicLayer.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassNest.ViewModels
{
    public partial class TwoFactorViewModel : ViewModelBase
    {
        private readonly IAuthProvider authProvider;
        private readonly DispatcherTimer countdownTimer;
        private DateTime expiresAt;
        private CancellationTokenSource? errorDismissCts;

        [ObservableProperty]
        private string maskedEmail = string.Empty;

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
        private string resendCountdown = "5:00";

        [ObservableProperty]
        private IBrush countdownColor = new SolidColorBrush(Color.Parse("#2AA26A"));

        [ObservableProperty]
        private bool isCodeExpired;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private bool hasError;

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

        public event Action? VerificationSucceeded;
        public event Action? BackRequested;

        public TwoFactorViewModel(IAuthProvider authProvider)
        {
            this.authProvider = authProvider;

            countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            countdownTimer.Tick += (_, _) => UpdateCountdown();

            StartCountdown();
        }

        private void StartCountdown()
        {
            expiresAt = DateTime.UtcNow.AddMinutes(5);
            IsCodeExpired = false;
            UpdateCountdown();
            countdownTimer.Start();
        }

        private void UpdateCountdown()
        {
            var remaining = expiresAt - DateTime.UtcNow;

            if (remaining <= TimeSpan.Zero)
            {
                ResendCountdown = "0:00";
                CountdownColor = new SolidColorBrush(Color.Parse("#D6503C"));
                IsCodeExpired = true;
                countdownTimer.Stop();
                return;
            }

            ResendCountdown = $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2}";

            var colorHex = remaining.TotalSeconds switch
            {
                > 150 => "#2AA26A",
                > 60 => "#E0952E",
                _ => "#D6503C"
            };

            CountdownColor = new SolidColorBrush(Color.Parse(colorHex));
        }

        private static string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 1) return email;
            return email[0] + new string('*', atIndex - 1) + email[atIndex..];
        }

        [RelayCommand]
        private void ConfirmCode()
        {
            var code = $"{Digit1}{Digit2}{Digit3}{Digit4}{Digit5}{Digit6}";
            ErrorMessage = null;

            if (!authProvider.VerifyTwoFactor(code))
            {
                ShowError("Neispravan ili istekao kod.");
                return;
            }

            VerificationSucceeded?.Invoke();
        }

        [RelayCommand]
        private void ResendCode()
        {
            authProvider.ResendTwoFactorCode();

            Digit1 = string.Empty;
            Digit2 = string.Empty;
            Digit3 = string.Empty;
            Digit4 = string.Empty;
            Digit5 = string.Empty;
            Digit6 = string.Empty;

            StartCountdown();
        }

        private async void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;

            errorDismissCts?.Cancel();
            var cts = new CancellationTokenSource();
            errorDismissCts = cts;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                HasError = false;
            }
            catch (TaskCanceledException)
            {
            }
        }

    }
}

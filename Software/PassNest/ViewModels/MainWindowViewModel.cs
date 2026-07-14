using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.PasswordGeneration;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PassNest.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IAuthProvider authProvider;
        private readonly IPasswordGenerator passwordGenerator;

        [ObservableProperty]
        private ViewModelBase currentPage;

        public MainWindowViewModel(IAuthProvider authProvider, IPasswordGenerator passwordGenerator)
        {
            this.authProvider = authProvider;
            this.passwordGenerator = passwordGenerator;
            CurrentPage = CreateInitialPage();
        }

        private ViewModelBase CreateInitialPage() => authProvider.HasRegisteredUser() ? CreateLoginPage() : (ViewModelBase)CreateRegisterPage();

        private LoginViewModel CreateLoginPage()
        {
            var vm = new LoginViewModel(authProvider);
            vm.LoginSucceded += OnAuthenticated;
            vm.TwoFacotrRequired += OnTwoFactorRequired;
            return vm;
        }

        private void OnTwoFactorRequired()
        {
            var vm = new TwoFactorViewModel(authProvider);
            vm.VerificationSucceeded += OnAuthenticated;
            vm.BackRequested += () => CurrentPage = CreateLoginPage();
            CurrentPage = vm;
        }

        private RegisterViewModel CreateRegisterPage()
        {
            var vm = new RegisterViewModel(authProvider, passwordGenerator);
            vm.RegistrationSucceeded += OnAuthenticated;
            return vm;
        }

        private void OnAuthenticated()
        {
            CurrentPage = new ShellViewModel();
        }
    }
}

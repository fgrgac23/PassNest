using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using BusinessLogicLayer;
using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Security;
using DataAccessLayer.Data;
using DataAccessLayer.Email;
using DataAccessLayer.Repository;
using EntityLayer;
using Microsoft.Extensions.DependencyInjection;
using PassNest.ViewModels;
using PassNest.Views;
using BusinessLogicLayer.PasswordGeneration;
using BusinessLogicLayer.AccountManagement;
using PassNest.Services;
using BusinessLogicLayer.Autofill;
using System;

namespace PassNest
{
    public partial class App : Application
    {
        private ServiceProvider provider = null;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            DatabaseInitializer.InitializeDatabase();

            var service = new ServiceCollection();
            service.AddSingleton<PassNestDbContext>();
            service.AddSingleton<ICryptoService, EncryptionEngine>();
            service.AddSingleton(new TwoFactorCodeGenerator());
            service.AddSingleton<IEmailSender>(new EmailNotifier(
                smtpHost: "smtp.gmail.com",
                smtpPort: 587,
                senderEmail: "passnest.2fa.info@gmail.com",
                senderPassword: "PassNest2FA@"));
            service.AddSingleton<IRepository<User>, Repository<User>>();
            service.AddSingleton<IRepository<Account>, Repository<Account>>();
            service.AddSingleton<IRepository<Category>, Repository<Category>>();
            service.AddSingleton<IAuthProvider, AuthenticationService>();
            service.AddSingleton<IPasswordGenerator, PasswordGenerator>();
            service.AddSingleton<IAccountStore, AccountManager>();
            service.AddSingleton<IClipboardService, ClipboardService>();
            service.AddSingleton<IAutofillEngine, AutofillEngine>();
            service.AddSingleton<MainWindowViewModel>();

            var provider = service.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = provider.GetRequiredService<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnTrayOpenClick(object? sender, EventArgs e)
        {
            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                desktop.MainWindow.WindowState = Avalonia.Controls.WindowState.Normal;
                desktop.MainWindow.Show();
                desktop.MainWindow.Activate();
            }
        }

        private void OnTrayExitClick(object? sender, EventArgs e)
        {
            provider.GetRequiredService<IAutofillEngine>().UnregisterHotkeys();
            Environment.Exit(0);
        }
    }
}
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

namespace PassNest
{
    public partial class App : Application
    {
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
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using BusinessLogicLayer;
using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.Authentication;
using BusinessLogicLayer.Autofill;
using BusinessLogicLayer.BaseBackup;
using BusinessLogicLayer.PasswordGeneration;
using BusinessLogicLayer.Security;
using DataAccessLayer.Backup;
using DataAccessLayer.Data;
using DataAccessLayer.Email;
using DataAccessLayer.Repository;
using EntityLayer;
using Microsoft.Extensions.DependencyInjection;
using PassNest.Services;
using PassNest.ViewModels;
using PassNest.Views;
using System;
using System.Linq;

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
            service.AddSingleton<IBackupStore, FileBackupStore>();
            service.AddSingleton<IBackupManager, BackupManager>();
            service.AddSingleton<IFIleDialogService, FileDialogService>();
            service.AddSingleton<MainWindowViewModel>();

            provider = service.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = provider.GetRequiredService<MainWindowViewModel>(),
                };
            }

            SetupTrayIcon();
            base.OnFrameworkInitializationCompleted();
        }

        private void SetupTrayIcon()
        {
            using var iconStream = AssetLoader.Open(new Uri("avares://PassNest/Assets/PassNest-icon.ico"));

            var menu = new NativeMenu();

            var openItem = new NativeMenuItem("Otvori");
            openItem.Click += OnTrayOpenClick;
            menu.Items.Add(openItem);

            var exitItem = new NativeMenuItem("Zatvori");
            exitItem.Click += OnTrayExitClick;
            menu.Items.Add(exitItem);

            var trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(iconStream),
                ToolTipText = "PassNest",
                Menu = menu
            };

            TrayIcon.SetIcons(this, new TrayIcons { trayIcon });
        }

        private void OnTrayOpenClick(object? sender, EventArgs e)
        {
            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
            {
                desktop.MainWindow.WindowState = WindowState.Normal;
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
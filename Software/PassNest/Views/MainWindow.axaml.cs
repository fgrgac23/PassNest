using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;

namespace PassNest.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Opened += (_, _) => CenterOnScreen();
            SizeChanged += (_, _) => CenterOnScreen();
            Closing += OnClosing;
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

            DataContextChanged += (_, _) =>
            {
                if (DataContext is PassNest.ViewModels.MainWindowViewModel vm)
                {
                    vm.ShowMainWindowRequested += OnShowMainWindowRequested;
                }
            };
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                OpenUserManual();
            }
        }

        private static void OpenUserManual()
        {
            var manualPath = Path.Combine(AppContext.BaseDirectory, "Docs", "PASSNEST-KorisnickiPrirucnik.pdf");

            if (!File.Exists(manualPath))
            {
                return;
            }

            Process.Start(new ProcessStartInfo(manualPath) { UseShellExecute = true });
        }

        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnShowMainWindowRequested()
        {
            Dispatcher.UIThread.Post(() =>
            {
                WindowState = WindowState.Normal;
                Show();
                Activate();
            });
        }

        private void CenterOnScreen()
        {
            var screen = Screens.ScreenFromVisual(this) ?? Screens.Primary;
            if (screen is null)
            {
                return;
            }

            var workingArea = screen.WorkingArea;
            var frameSize = FrameSize ?? ClientSize;
            var pixelWidth = (int)(frameSize.Width * RenderScaling);
            var pixelHeight = (int)(frameSize.Height * RenderScaling);

            var x = workingArea.X + (workingArea.Width - pixelWidth) / 2;
            var y = workingArea.Y + (workingArea.Height - pixelHeight) / 2;

            Position = new Avalonia.PixelPoint(x, y);
        }
    }
}
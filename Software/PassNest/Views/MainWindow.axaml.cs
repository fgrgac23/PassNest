using Avalonia.Controls;
using Avalonia.Threading;
using System;

namespace PassNest.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += (_, _) => CenterOnScreen();
            Closing += OnClosing;

            DataContextChanged += (_, _) =>
            {
                if (DataContext is PassNest.ViewModels.MainWindowViewModel vm)
                {
                    vm.ShowMainWindowRequested += OnShowMainWindowRequested;
                }
            };
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
            if(screen is null)
            {
                return;
            }

            var workingArea = screen.WorkingArea;
            var pixelWidth = (int)(ClientSize.Width * RenderScaling);
            var pixelHeight = (int)(ClientSize.Height * RenderScaling);

            var x = workingArea.X + (workingArea.Width - pixelWidth) / 2;
            var y = workingArea.Y + (workingArea.Height - pixelHeight) / 2;

            Position = new Avalonia.PixelPoint(x, y);
        }
    }
}
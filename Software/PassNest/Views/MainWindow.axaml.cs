using Avalonia.Controls;
using System;

namespace PassNest.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += (_, _) => CenterOnScreen();
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
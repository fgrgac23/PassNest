using Avalonia.Controls;
using Avalonia.Input;

namespace PassNest.Views
{
    public partial class TwoFactorView : UserControl
    {
        private TextBox[] digitBoxes = System.Array.Empty<TextBox>();

        public TwoFactorView()
        {
            InitializeComponent();

            digitBoxes = new[]
            {
                Digit1Box, Digit2Box, Digit3Box, Digit4Box, Digit5Box, Digit6Box
            };

            for(var i = 0; i < digitBoxes.Length; i++)
            {
                var index = i;
                digitBoxes[i].AddHandler(KeyDownEvent, (_, e) => OnDigitKeyDown(index, e), Avalonia.Interactivity.RoutingStrategies.Tunnel);
                digitBoxes[i].TextChanged += (_, _) => OnDigitTextChanged(index);
            }
        }

        private void OnDigitTextChanged(int index)
        {
            var box = digitBoxes[index];
            if (box.Text?.Length == 1 && index < digitBoxes.Length - 1)
            {
                digitBoxes[index + 1].Focus();
                digitBoxes[index + 1].SelectAll();
            }
        }

        private void OnDigitKeyDown(int index, KeyEventArgs e)
        {
            if (e.Key == Key.Back && string.IsNullOrEmpty(digitBoxes[index].Text) && index > 0)
            {
                digitBoxes[index - 1].Focus();
                digitBoxes[index - 1].SelectAll();
            }
        }
    }
}

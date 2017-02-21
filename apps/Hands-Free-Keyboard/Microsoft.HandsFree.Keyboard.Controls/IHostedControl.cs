using System.Windows;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    interface IHostedControl : IGazeTarget
    {
        FrameworkElement Button { get; }

        string Keytop { get; set; }

        IKeyboardHost KeyboardHost { get; set; }

        void SetMultiplier(double multiplier, double repeatMultiplier);
    }
}

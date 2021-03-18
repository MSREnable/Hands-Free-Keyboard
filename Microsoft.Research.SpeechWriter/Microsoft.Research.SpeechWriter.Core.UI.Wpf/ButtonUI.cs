using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.Core.UI.Wpf
{
    public class ButtonUI : Button, IButtonUI
    {
        double IButtonUI.RenderedWidth => ActualWidth;
    }
}

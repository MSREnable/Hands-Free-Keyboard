using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    public class ButtonUI : Button, IButtonUI
    {
        double IButtonUI.RenderedWidth => ActualWidth;
    }
}

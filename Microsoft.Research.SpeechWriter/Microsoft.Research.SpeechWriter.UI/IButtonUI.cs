using System.Drawing;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface IButtonUI
    {
        double RenderedWidth { get; }

        RectangleF GetRenderedRectangle();
    }
}
using Microsoft.Research.SpeechWriter.Core;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public static class ColorHelper
    {
        private static readonly Brush None = null;
        private static readonly Brush Text = new SolidColorBrush(Colors.Black);
        private static readonly Brush Gray = new SolidColorBrush(Colors.Gray);
        private static readonly Brush HeadBackground = new SolidColorBrush(Colors.LightGreen);
        private static readonly Brush SuggestionBackground = new SolidColorBrush(Colors.LightYellow);
        private static readonly Brush SuggestionPartBackground = new SolidColorBrush(Colors.LightSteelBlue);
        private static readonly Brush SymbolBackground = new SolidColorBrush(Colors.Blue);
        private static readonly Brush GhostBackground = new SolidColorBrush(Colors.LightBlue);

        public static Brush ToBrush(this TileColor color)
        {
            Brush brush;

            switch (color)
            {
                case TileColor.Text: brush = Text; break;
                case TileColor.GrayText: brush = Gray; break;
                case TileColor.HeadBackground: brush = HeadBackground; break;
                case TileColor.SuggestionBackground: brush = SuggestionBackground; break;
                case TileColor.SuggestionPartBackground: brush = SuggestionPartBackground; break;
                case TileColor.SymbolBackground: brush = SymbolBackground; break;
                case TileColor.GhostBackground: brush = GhostBackground; break;

                case TileColor.None:
                default:
                    Debug.Assert(color == TileColor.None);
                    brush = None;
                    break;
            }

            return brush;
        }
    }
}

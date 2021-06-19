using Microsoft.Research.SpeechWriter.Core;
using System.Drawing;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.UI.Test
{
    internal class MockButton : IButtonUI
    {
        private readonly ICommand _command;

        public MockButton(int id, ITile tile, double width, double height, WidthBehavior behavior)
        {
            Id = id;

            _command = tile;

            Width = width;
            Height = height;
            ;
            if (behavior == WidthBehavior.Minimum)
            {
                var captionWidth = 6 + 2 * tile.Content.Length;
                if (Width < captionWidth)
                {
                    Width = captionWidth;
                }
            }
        }

        internal int Id { get; }

        internal double X { get; set; }

        internal double Y { get; set; }

        internal double Width { get; }

        internal double Height { get; }

        internal void Invoke() => _command.Execute(null);

        double IButtonUI.RenderedWidth => Width;

        RectangleF IButtonUI.GetRenderedRectangle() => new RectangleF((float)X, (float)Y, (float)Width, (float)Height);
    }
}

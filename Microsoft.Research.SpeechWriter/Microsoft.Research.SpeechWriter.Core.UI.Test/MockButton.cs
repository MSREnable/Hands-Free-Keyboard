using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI.Test
{
    class MockButton : IButtonUI
    {
        private readonly ICommand _command;

        public MockButton(int id, ICommand command, double width, double height, WidthBehavior behavior)
        {
            Id = id;

            _command = command;

            Width = width;
            Height = height;
            ;
            if (behavior == WidthBehavior.Minimum)
            {
                var captionWidth = 6 + 2 * command.ToString().Length;
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
    }
}

using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public abstract class ButtonPanel<T>
        where T : IButtonUI
    {
        readonly IButtonSurfaceUI<T> _surface;
        readonly ApplicationLayout<T> _layout;

        private double _left;

        private double _top;

        protected ButtonPanel(ApplicationLayout<T> layout)
        {
            _surface = layout.Surface;
            _layout = layout;
        }

        protected double UniformMargin => _layout.UniformMargin;

        protected double Width { get; private set; }

        protected int Rows { get; private set; }

        internal void Move(double x, double y, double width, int rows)
        {
            _left = x;
            _top = y;
            Width = width;
            Rows = rows;

            ResetContent();
        }

        protected abstract void ResetContent();

        protected T Create(ICommand command, WidthBehavior behavior)
        {
            var element = _surface.Create(command, _layout.Pitch, _layout.Pitch, behavior);
            return element;
        }

        protected void Move(T element, int row, double offset)
        {
            _surface.Move(element, _left + offset, _top + row * (_layout.Pitch + _layout.UniformMargin));
        }

        protected void Remove(T element)
        {
            _surface.Remove(element);
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public abstract class ButtonPanel<TButton, TItem>
        where TButton : IButtonUI
    {
        readonly IButtonSurfaceUI<TButton> _surface;
        readonly ApplicationLayout<TButton> _layout;

        private readonly ReadOnlyObservableCollection<TItem> _list;

        private double _left;

        private double _top;

        protected ButtonPanel(ApplicationLayout<TButton> layout, ReadOnlyObservableCollection<TItem> list)
        {
            _surface = layout.Surface;
            _layout = layout;
            _list = list;

            ((INotifyCollectionChanged)_list).CollectionChanged += (s, e) => ResetContent(_list);
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

            ResetContent(_list);
        }

        protected abstract void ResetContent(IList<TItem> list);

        protected TButton Create(ICommand command, WidthBehavior behavior)
        {
            var element = _surface.Create(command, _layout.Pitch, _layout.Pitch, behavior);
            return element;
        }

        protected void Move(TButton element, int row, double offset)
        {
            _surface.Move(element, _left + offset, _top + row * (_layout.Pitch + _layout.UniformMargin));
        }

        protected void Remove(TButton element)
        {
            _surface.Remove(element);
        }
    }
}

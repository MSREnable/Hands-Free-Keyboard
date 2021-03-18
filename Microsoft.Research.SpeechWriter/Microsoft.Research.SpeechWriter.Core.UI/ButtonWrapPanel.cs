using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ButtonWrapPanel<T> : ButtonPanel<T>
        where T : IButtonUI
    {
        private readonly ReadOnlyObservableCollection<ICommand> _list;

        private readonly List<T> _elementList = new List<T>();

        public ButtonWrapPanel(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ICommand> list)
            : base(layout)
        {
            _list = list;

            ((INotifyCollectionChanged)_list).CollectionChanged += (s, e) => ResetContent();
        }

        protected override void ResetContent()
        {
            foreach (var element in _elementList)
            {
                Remove(element);
            }
            _elementList.Clear();

            var row = 0;
            var offset = 0.0;

            using (var enumerator = _list.GetEnumerator())
            {
                while (enumerator.MoveNext() && row < Rows)
                {
                    var item = enumerator.Current;
                    var element = Create(item, WidthBehavior.Minimum);

                    var nextOffset = offset + element.RenderedWidth + UniformMargin;

                    if (Width < nextOffset)
                    {
                        row++;
                        offset = 0.0;
                        nextOffset = element.RenderedWidth + UniformMargin;
                    }

                    if (row < Rows)
                    {
                        Move(element, row, offset);
                        _elementList.Add(element);
                    }
                    else
                    {
                        Remove(element);
                    }

                    offset = nextOffset;
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ButtonListColumn<T> : ButtonPanel<T>
        where T : IButtonUI
    {
        private readonly ReadOnlyObservableCollection<IEnumerable<ICommand>> _list;

        private readonly List<List<T>> _elementLists = new List<List<T>>();

        public ButtonListColumn(ApplicationLayout<T> layout, ReadOnlyObservableCollection<IEnumerable<ICommand>> list)
            : base(layout)
        {
            _list = list;

            ((INotifyCollectionChanged)_list).CollectionChanged += (s, e) => ResetContent();
        }

        protected override void ResetContent()
        {
            foreach (var elementList in _elementLists)
            {
                foreach (var element in elementList)
                {
                    Remove(element);
                }
            }
            _elementLists.Clear();

            var row = 0;
            foreach (var enumerable in _list)
            {
                var elementList = new List<T>();

                using (var enumerator = enumerable.GetEnumerator())
                {
                    var offset = 0.0;

                    while (offset < Width && enumerator.MoveNext())
                    {
                        var command = enumerator.Current;
                        var element = Create(command, WidthBehavior.Minimum);

                        var nextOffset = offset + element.RenderedWidth + UniformMargin;
                        if (nextOffset < Width)
                        {
                            elementList.Add(element);
                            Move(element, row, offset);
                        }
                        else
                        {
                            Remove(element);
                        }

                        offset = nextOffset;
                    }
                }

                row++;

                _elementLists.Add(elementList);
            }
        }
    }
}

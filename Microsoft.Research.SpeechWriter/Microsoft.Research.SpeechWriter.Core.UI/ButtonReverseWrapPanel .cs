using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ButtonReverseWrapPanel<T> : ButtonPanel<T>
        where T : IButtonUI
    {
        private readonly ReadOnlyObservableCollection<ICommand> _list;

        private readonly List<T> _elementList = new List<T>();

        public ButtonReverseWrapPanel(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ICommand> list)
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

            using (var enumerator = _list.GetEnumerator())
            {
                var offset = Width;

                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    var element = Create(item, WidthBehavior.Minimum);
                    _elementList.Add(element);

                    offset -= element.RenderedWidth + UniformMargin;
                    Move(element, 0, offset);
                }
            }
        }
    }
}

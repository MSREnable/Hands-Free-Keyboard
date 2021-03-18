using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ButtonColumn<T> : ButtonPanel<T>
        where T : IButtonUI
    {
        private readonly ReadOnlyObservableCollection<ICommand> _list;

        private readonly List<T> _elements = new List<T>();

        public ButtonColumn(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ICommand> list)
            : base(layout)
        {
            _list = list;

            ((INotifyCollectionChanged)_list).CollectionChanged += (s, e) => ResetContent();
            ResetContent();
        }

        protected override void ResetContent()
        {
            foreach (var element in _elements)
            {
                Remove(element);
            }
            _elements.Clear();

            var row = 0;
            foreach (var command in _list)
            {
                var element = Create(command, WidthBehavior.Fixed);
                Move(element, row, 0);
                _elements.Add(element);

                row++;
            }
        }
    }
}

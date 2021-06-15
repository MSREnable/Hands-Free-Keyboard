using Microsoft.Research.SpeechWriter.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class ButtonColumn<T> : ButtonPanel<T, ITile>
        where T : IButtonUI
    {
        private readonly List<T> _elements = new List<T>();

        public ButtonColumn(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ITile> list)
            : base(layout, list)
        {
        }

        protected override void AddContent(IList<ITile> list, int startIndex, int count)
        {
            for (var i = 0; i < count; i++)
            {
                AddItem(startIndex + i, list[startIndex + i]);
            }
        }

        protected override void ResetContent(IList<ITile> list)
        {
            foreach (var element in _elements)
            {
                Remove(element);
            }
            _elements.Clear();

            var row = 0;
            foreach (var command in list)
            {
                AddItem(row, command);

                row++;
            }
        }

        private void AddItem(int row, ITile command)
        {
            var element = Create(command, WidthBehavior.Fixed);
            Move(element, row, 0);
            _elements.Add(element);
        }
    }
}

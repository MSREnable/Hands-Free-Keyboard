using Microsoft.Research.SpeechWriter.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class ButtonWrapPanel<T> : ButtonPanel<T, ITile>
        where T : IButtonUI
    {
        private readonly List<T> _elementList = new List<T>();

        public ButtonWrapPanel(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ITile> list)
            : base(layout, list)
        {
        }

        protected override void ResetContent(IList<ITile> list)
        {
            foreach (var element in _elementList)
            {
                Remove(element);
            }
            _elementList.Clear();

            var row = 0;
            var offset = 0.0;

            using (var enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext() && row < Rows)
                {
                    var item = enumerator.Current;
                    var element = Create(item, WidthBehavior.Minimum);

                    var nextOffset = offset + element.RenderedWidth;

                    if (Width < nextOffset)
                    {
                        row++;
                        offset = 0.0;
                        nextOffset = element.RenderedWidth;
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

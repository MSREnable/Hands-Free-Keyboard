using Microsoft.Research.SpeechWriter.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class ButtonReverseWrapPanel<T> : ButtonPanel<T, ITile>
        where T : IButtonUI
    {
        private readonly List<T> _elementList = new List<T>();

        public ButtonReverseWrapPanel(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ITile> list)
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

            using (var enumerator = list.GetEnumerator())
            {
                var offset = Width;

                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    var element = Create(item, WidthBehavior.Minimum);
                    _elementList.Add(element);

                    offset -= element.RenderedWidth;
                    Move(element, 0, offset);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ButtonWrapPanel<T> : ButtonPanel<T, ICommand>
        where T : IButtonUI
    {
        private readonly List<T> _elementList = new List<T>();

        public ButtonWrapPanel(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ICommand> list)
            : base(layout, list)
        {
        }

        protected override void ResetContent(IList<ICommand> list)
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

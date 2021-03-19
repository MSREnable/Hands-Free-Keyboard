using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ButtonReverseWrapPanel<T> : ButtonPanel<T, ICommand>
        where T : IButtonUI
    {
        private readonly List<T> _elementList = new List<T>();

        public ButtonReverseWrapPanel(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ICommand> list)
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

            using (var enumerator = list.GetEnumerator())
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

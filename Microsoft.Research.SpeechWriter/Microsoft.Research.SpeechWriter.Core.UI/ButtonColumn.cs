﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ButtonColumn<T> : ButtonPanel<T, ICommand>
        where T : IButtonUI
    {
        private readonly List<T> _elements = new List<T>();

        public ButtonColumn(ApplicationLayout<T> layout, ReadOnlyObservableCollection<ICommand> list)
            : base(layout, list)
        {
        }

        protected override void ResetContent(IList<ICommand> list)
        {
            foreach (var element in _elements)
            {
                Remove(element);
            }
            _elements.Clear();

            var row = 0;
            foreach (var command in list)
            {
                var element = Create(command, WidthBehavior.Fixed);
                Move(element, row, 0);
                _elements.Add(element);

                row++;
            }
        }
    }
}

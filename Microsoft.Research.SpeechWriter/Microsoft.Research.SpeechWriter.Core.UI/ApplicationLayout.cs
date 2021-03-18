using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
{
    public class ApplicationLayout<T>
        where T : IButtonUI
    {
        private readonly IButtonSurfaceUI<T> _surface;

        private readonly ButtonWrapPanel<T> _documentWrapPanel;

        private readonly ButtonReverseWrapPanel<T> _documentTailPanel;

        private readonly ButtonColumn<T> _navigationColumn;

        private readonly ButtonListColumn<T> _selectionListsColumn;

        private readonly double _pitch;
        private readonly double _uniformMargin;
        private int _rows;

        private readonly ObservableCollection<ICommand> _documentList = new ObservableCollection<ICommand>();

        private readonly ObservableCollection<ICommand> _documentTailList = new ObservableCollection<ICommand>();

        private readonly ObservableCollection<ICommand> _navigationList = new ObservableCollection<ICommand>();

        private readonly ObservableCollection<IEnumerable<ICommand>> _selectionLists = new ObservableCollection<IEnumerable<ICommand>>();

        public ApplicationLayout(IButtonSurfaceUI<T> surface, double pitch, double uniformMargin)
        {
            _surface = surface;
            _pitch = pitch;
            _uniformMargin = uniformMargin;

            _documentWrapPanel = new ButtonWrapPanel<T>(this, new ReadOnlyObservableCollection<ICommand>(_documentList));
            _documentTailPanel = new ButtonReverseWrapPanel<T>(this, new ReadOnlyObservableCollection<ICommand>(_documentTailList));
            _navigationColumn = new ButtonColumn<T>(this, new ReadOnlyObservableCollection<ICommand>(_navigationList));
            _selectionListsColumn = new ButtonListColumn<T>(this, new ReadOnlyObservableCollection<IEnumerable<ICommand>>(_selectionLists));

            _surface.Resized += OnResized;

            var random = new Random(-1);
            for (var i = 0; i < 20; i++)
            {
                _documentList.Add(new PsuedoContent(random.Next().ToString()));
            }

            _documentTailList.Add(new PsuedoContent(">"));
        }

        internal IButtonSurfaceUI<T> Surface => _surface;

        internal double Pitch => _pitch;

        internal double UniformMargin => _uniformMargin;

        private double WingWidth => (_surface.TotalWidth - _pitch - 2 * _uniformMargin) / 2;

        private void OnResized(object sender, EventArgs e)
        {
            _rows = (int)Math.Floor((_surface.TotalHeight - _uniformMargin) / (_pitch + _uniformMargin));

            while (_rows < _navigationList.Count)
            {
                _navigationList.RemoveAt(_navigationList.Count - 1);
            }
            while (_navigationList.Count < _rows)
            {
                _navigationList.Add(new PsuedoContent((_navigationList.Count + 1).ToString()));
            }

            while (_rows - 1 < _selectionLists.Count)
            {
                _selectionLists.RemoveAt(_selectionLists.Count - 1);
            }
            while (_selectionLists.Count < _rows - 1)
            {
                _selectionLists.Add(GetSelectionList(_selectionLists.Count));
            }

            _documentWrapPanel.Move(x: _uniformMargin, y: _uniformMargin, width: WingWidth, _rows - 1);
            _documentTailPanel.Move(x: _uniformMargin, y: _uniformMargin + (_rows - 1) * (_pitch + _uniformMargin), width: WingWidth, 1);
            _navigationColumn.Move(x: _uniformMargin + WingWidth, y: _uniformMargin, width: _pitch, _rows);
            _selectionListsColumn.Move(x: _surface.TotalWidth - WingWidth, y: _uniformMargin + (_uniformMargin + _pitch) / 2, width: WingWidth, _rows - 1);
        }

        private IEnumerable<ICommand> GetSelectionList(int seed)
        {
            var random = new Random(seed);
            for (; ; )
            {
                var value = random.Next();
                yield return new PsuedoContent(value.ToString());
            }
        }
    }
}

using Microsoft.Research.SpeechWriter.Core;
using System;

namespace Microsoft.Research.SpeechWriter.UI
{
    public class ApplicationLayout<T>
        where T : IButtonUI
    {
        private readonly IButtonSurfaceUI<T> _surface;

        private readonly ButtonWrapPanel<T> _documentWrapPanel;

        private readonly ApplicationModel _model = new ApplicationModel();

        private readonly ButtonReverseWrapPanel<T> _documentTailPanel;

        private readonly ButtonColumn<T> _navigationColumn;

        private readonly ButtonListColumn<T> _selectionListsColumn;

        private readonly double _pitch;
        private readonly double _uniformMargin;
        private int _rows;

        public ApplicationLayout(IButtonSurfaceUI<T> surface, double pitch, double uniformMargin)
        {
            _surface = surface;
            _pitch = pitch;
            _uniformMargin = uniformMargin;

            _documentWrapPanel = new ButtonWrapPanel<T>(this, _model.HeadItems);
            _documentTailPanel = new ButtonReverseWrapPanel<T>(this, _model.TailItems);
            _navigationColumn = new ButtonColumn<T>(this, _model.SuggestionInterstitials);
            _selectionListsColumn = new ButtonListColumn<T>(this, _model.SuggestionLists);

            _surface.Resized += OnResized;
        }

        internal IButtonSurfaceUI<T> Surface => _surface;

        internal double Pitch => _pitch;

        internal double UniformMargin => _uniformMargin;

        private double WingWidth => (_surface.TotalWidth - _pitch - 2 * _uniformMargin) / 2;

        private void OnResized(object sender, EventArgs e)
        {
            _rows = (int)Math.Floor((_surface.TotalHeight - _uniformMargin) / (_pitch + _uniformMargin));

            _model.MaxNextSuggestionsCount = _rows;

            _documentWrapPanel.Move(x: _uniformMargin, y: _uniformMargin, width: WingWidth, _rows - 1);
            _documentTailPanel.Move(x: _uniformMargin, y: _uniformMargin + (_rows - 1) * (_pitch + _uniformMargin), width: WingWidth, 1);
            _navigationColumn.Move(x: _uniformMargin + WingWidth, y: _uniformMargin, width: _pitch, _rows);
            _selectionListsColumn.Move(x: _surface.TotalWidth - WingWidth, y: _uniformMargin + (_uniformMargin + _pitch) / 2, width: WingWidth, _rows - 1);
        }
    }
}

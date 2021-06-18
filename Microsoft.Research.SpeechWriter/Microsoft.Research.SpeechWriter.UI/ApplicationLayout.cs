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
        private int _rows;

        public ApplicationLayout(ApplicationModel model, IButtonSurfaceUI<T> surface, double pitch)
        {
            _model = model;
            _surface = surface;
            _pitch = pitch;

            _documentWrapPanel = new ButtonWrapPanel<T>(this, _model.HeadItems);
            _documentTailPanel = new ButtonReverseWrapPanel<T>(this, _model.TailItems);
            _navigationColumn = new ButtonColumn<T>(this, _model.SuggestionInterstitials);
            _selectionListsColumn = new ButtonListColumn<T>(this, _model.SuggestionLists);

            _surface.Resized += OnResized;
        }

        public ApplicationLayout(IButtonSurfaceUI<T> surface, double pitch)
            : this(new ApplicationModel(), surface, pitch)
        {
        }

        internal IButtonSurfaceUI<T> Surface => _surface;

        internal double Pitch => _pitch;

        private double WingWidth => (_surface.TotalWidth - _pitch) / 2;

        private void OnResized(object sender, EventArgs e)
        {
            _rows = (int)Math.Floor(_surface.TotalHeight / _pitch);

            _model.MaxNextSuggestionsCount = _rows;

            _documentWrapPanel.Move(x: 0, y: 0, width: WingWidth, _rows - 1);
            _documentTailPanel.Move(x: 0, y: (_rows - 1) * _pitch, width: WingWidth, 1);
            _navigationColumn.Move(x: WingWidth, y: 0, width: _pitch, _rows);
            _selectionListsColumn.Move(x: _surface.TotalWidth - WingWidth, y: _pitch / 2, width: WingWidth, _rows - 1);
        }
    }
}

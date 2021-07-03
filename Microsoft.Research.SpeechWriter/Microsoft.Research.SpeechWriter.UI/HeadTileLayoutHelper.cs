using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.Research.SpeechWriter.UI
{
    internal class HeadTileLayoutHelper<TControl, TSize, TRect> : TileLayoutHelper<TControl, TSize, TRect>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        private readonly SuperPanelHelper<TControl, TSize, TRect> _superHelper;

        internal HeadTileLayoutHelper(SuperPanelHelper<TControl, TSize, TRect> superHelper,
            ReadOnlyObservableCollection<ITile> list)
            : base(superHelper, list)
        {
            _superHelper = superHelper;
        }

        public override TSize ArrangeOverride(TSize finalSize)
        {
            var horizontalPitch = _superHelper.HorizontalPitch;
            var verticalPitch = _superHelper.VerticalPitch;

            var panelWidth = _panel.GetWidth(finalSize);
            var panelHeight = _panel.GetHeight(finalSize);
            var availableRows = (int)Math.Round(panelHeight / verticalPitch);

            var x = 0.0;
            var y = 0.0;

            var rects = new TRect[_controls.Count];
            var rows = 1;

            var controlsCount = _controls.Count;
            for (var index = 0; index < controlsCount; index++)
            {
                var control = _controls[index];
                var controlSize = _panel.GetDesiredSize(control);
                var controlWidth = _panel.GetWidth(controlSize);
                var controlHeight = _panel.GetHeight(controlSize);

                var controlRight = x + controlWidth;
                if (panelWidth < controlRight && x != 0.0)
                {
                    x = 0;
                    y += controlHeight;
                    rows++;
                }

                var rect = _panel.CreateRect(x, y, controlWidth, controlHeight);
                rects[index] = rect;

                x += controlWidth;
            }

            if (availableRows < rows)
            {
                var emptyRect = _panel.CreateRect(0, 0, 0, 0);

                var selectedIndex = _superHelper.SelectedHeadIndex;
                var selectedRect = rects[selectedIndex];
                var selectedY = _panel.GetY(selectedRect);
                var selectedRow = (int)Math.Round(selectedY / verticalPitch);

                if (availableRows <= selectedRow)
                {
                    // Need to trim some lines near the top.

                    var rowsToTrim = selectedRow - availableRows + 1;
                    var topTopRowToTrim = (selectedRow - rowsToTrim) / 2;
                    var yTrimMin = verticalPitch * (topTopRowToTrim - 0.5);

                    var trimIndex = 0;
                    while (trimIndex < rects.Length && _panel.GetY(rects[trimIndex]) < yTrimMin)
                    {
                        trimIndex++;
                    }

                    var yTrimMax = verticalPitch * (topTopRowToTrim + rowsToTrim - 0.5);
                    while (trimIndex < rects.Length && _panel.GetY(rects[trimIndex]) < yTrimMax)
                    {
                        rects[trimIndex] = emptyRect;
                        trimIndex++;
                    }

                    while (trimIndex < rects.Length)
                    {
                        var trimAmount = verticalPitch * rowsToTrim;

                        var rect = rects[trimIndex];
                        var rectX = _panel.GetX(rect);
                        var rectY = _panel.GetY(rect);
                        var width = _panel.GetWidth(rect);
                        var height = _panel.GetHeight(rect);
                        rects[trimIndex] = _panel.CreateRect(rectX, rectY - trimAmount, width, height);

                        trimIndex++;
                    }

                    rows -= rowsToTrim;
                }

                if (availableRows < rows)
                {
                    // Need to trim some lines near the bottom.

                    var maxTop = verticalPitch * (availableRows - 0.5);

                    for (var index = rects.Length - 1; 0 <= index && maxTop < _panel.GetY(rects[index]); index--)
                    {
                        rects[index] = emptyRect;
                    }
                }
            }

            for (var index = 0; index < controlsCount; index++)
            {
                var control = _controls[index];
                var rect = rects[index];

                _panel.Arrange(control, rect);
            }

            return finalSize;
        }
    }
}

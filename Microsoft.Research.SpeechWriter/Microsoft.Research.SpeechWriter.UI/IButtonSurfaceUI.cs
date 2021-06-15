using Microsoft.Research.SpeechWriter.Core;
using System;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface IButtonSurfaceUI<TElementUI>
        where TElementUI : IButtonUI
    {
        double TotalWidth { get; }

        double TotalHeight { get; }

        event EventHandler Resized;

        TElementUI Create(ITile tile, double width, double height, WidthBehavior behavior);

        void Move(TElementUI element, double x, double y);

        void Remove(TElementUI element);
    }
}

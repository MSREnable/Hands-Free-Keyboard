using System;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI
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

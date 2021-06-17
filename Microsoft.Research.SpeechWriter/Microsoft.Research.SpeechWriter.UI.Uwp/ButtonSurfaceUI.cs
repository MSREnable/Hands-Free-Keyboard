using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public class ButtonSurfaceUI : Canvas, IButtonSurfaceUI<TileButton>
    {
        double IButtonSurfaceUI<TileButton>.TotalWidth => ActualWidth;

        double IButtonSurfaceUI<TileButton>.TotalHeight => ActualHeight;

        event EventHandler IButtonSurfaceUI<TileButton>.Resized
        {
            add
            {
                _resized += value;
            }

            remove
            {
                _resized -= value; ;
            }
        }
        private EventHandler _resized;

        public ButtonSurfaceUI()
        {
            SizeChanged += (s, e) => _resized?.Invoke(s, EventArgs.Empty);
        }

        TileButton IButtonSurfaceUI<TileButton>.Create(ITile tile, double width, double height, WidthBehavior behavior)
        {
            var element = new TileButton
            {
                Item = tile,
                Height = height
            };
            switch (behavior)
            {
                case WidthBehavior.Fixed:
                    element.Width = width;
                    break;
                case WidthBehavior.Minimum:
                default:
                    Debug.Assert(behavior == WidthBehavior.Minimum);
                    element.MinWidth = width;
                    break;
            }
            Children.Add(element);
            if (behavior == WidthBehavior.Minimum)
            {
                element.Measure(new Size(MaxWidth, MaxHeight));
            }
            return element;
        }

        void IButtonSurfaceUI<TileButton>.Remove(TileButton element)
        {
            Children.Remove(element);
        }

        void IButtonSurfaceUI<TileButton>.Move(TileButton element, double x, double y)
        {
            SetLeft(element, x);
            SetTop(element, y);
        }
    }
}

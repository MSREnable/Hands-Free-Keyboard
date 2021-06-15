using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    public class ButtonSurfaceUI : Canvas, IButtonSurfaceUI<ButtonUI>
    {
        private static readonly TemplateOpacityCoverter _opacityConverter = new TemplateOpacityCoverter();

        double IButtonSurfaceUI<ButtonUI>.TotalWidth => ActualWidth;

        double IButtonSurfaceUI<ButtonUI>.TotalHeight => ActualHeight;

        event EventHandler IButtonSurfaceUI<ButtonUI>.Resized
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
            var dictionary = new ResourceDictionary();
            dictionary.Source = new Uri("pack://application:,,,/Microsoft.Research.SpeechWriter.UI.Wpf;component/TemplateDictionary.xaml");
            Resources.MergedDictionaries.Add(dictionary);

            SizeChanged += (s, e) => _resized?.Invoke(s, EventArgs.Empty);
        }

        ButtonUI IButtonSurfaceUI<ButtonUI>.Create(ITile tile, double width, double height, WidthBehavior behavior)
        {
            var element = new ButtonUI
            {
                Content = tile,
                Command = tile,
                Opacity = (double)_opacityConverter.Convert(tile, null, null, null),
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
                element.UpdateLayout();
            }
            return element;
        }

        void IButtonSurfaceUI<ButtonUI>.Remove(ButtonUI element)
        {
            Children.Remove(element);
        }

        void IButtonSurfaceUI<ButtonUI>.Move(ButtonUI element, double x, double y)
        {
            SetLeft(element, x);
            SetTop(element, y);
        }
    }
}

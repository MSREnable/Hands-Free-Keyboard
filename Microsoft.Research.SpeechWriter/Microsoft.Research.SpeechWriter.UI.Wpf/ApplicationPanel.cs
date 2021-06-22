﻿using Microsoft.Research.SpeechWriter.Core;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Research.SpeechWriter.UI.Wpf
{
    public class ApplicationPanel : Panel, IApplicationPanel<Control, Size, Rect>
    {
        private readonly ApplicationPanelHelper<Control, Size, Rect> _helper;

        public ApplicationPanel()
        {
            _helper = new ApplicationPanelHelper<Control, Size, Rect>(this);
        }

        public ApplicationModel Model
        {
            get => (ApplicationModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(ApplicationModel), typeof(ApplicationPanel),
            new PropertyMetadata(null, SetHelperModel));

        private static void SetHelperModel(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (ApplicationPanel)d;
            panel._helper.SetModel(panel.Model);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = _helper.MeasureOverride(availableSize);
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = _helper.ArrangeOverride(finalSize);
            return size;
        }

        void IApplicationPanel<Control, Size, Rect>.ResetControls()
        {
            Children.Clear();
        }

        Control IApplicationPanel<Control, Size, Rect>.CreateControl(ITile tile)
        {
            var button = new TileButton { Item = tile };
            Children.Add(button);
            return button;
        }

        void IApplicationPanel<Control, Size, Rect>.DeleteControl(Control control)
        {
            Children.Remove(control);
        }

        Size IApplicationPanel<Control, Size, Rect>.ToTSize(double width, double height)
        {
            return new Size(width, height);
        }

        double IApplicationPanel<Control, Size, Rect>.WidthFromTSize(Size size)
        {
            return size.Width;
        }

        double IApplicationPanel<Control, Size, Rect>.HeightFromTSize(Size size)
        {
            return size.Height;
        }

        Rect IApplicationPanel<Control, Size, Rect>.ToTRect(double x, double y, Size size)
        {
            return new Rect(x, y, size.Width, size.Height);
        }
    }
}

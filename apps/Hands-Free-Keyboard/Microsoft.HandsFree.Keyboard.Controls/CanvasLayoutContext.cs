using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.HandsFree.Keyboard.Controls.Layout;
using System.Diagnostics;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    class CanvasLayoutContext : LayoutContext<UserControl>
    {
        readonly IKeyboardHost _host;

        readonly Canvas _canvas;

        readonly double _fontSizeMultiplier;

        readonly static TextDecorationCollection _strikethrough = CreateStrikeThrough();

        internal CanvasLayoutContext(IKeyboardHost host, Canvas canvas, KeyboardLayout layout, double totalWidth, double totalHeight, double keySize, double fontSizeMultiplier)
            : base(layout, totalWidth, totalHeight, keySize)
        {
            _host = host;
            _canvas = canvas;
            _fontSizeMultiplier = fontSizeMultiplier;
        }

        static TextDecorationCollection CreateStrikeThrough()
        {
            var textDecoration = new TextDecoration
            {
                Location = TextDecorationLocation.Strikethrough,
                PenOffset = +0.3,
                PenOffsetUnit = TextDecorationUnit.FontRecommended
            };
            var textDecorations = new TextDecorationCollection();
            textDecorations.Add(textDecoration);

            return textDecorations;
        }

        Style GetStyle(IndividualKeyLayout layout)
        {
            Style style;

            try
            {
                style = (Style)Application.Current.TryFindResource(layout.Style);
            }
            catch
            {
                Debug.WriteLine($"Resource could not be loaded: {layout.Style}");

                style = null;
            }

            return style;
        }

        TControl CreateControl<TControl>(NonGapKeyLayout layout, double left, double top, double width, double height, bool isVisible)
            where TControl : Control, IHostedControl, new()
        {
            var control = new TControl
            {
                Keytop = layout.Caption,
                Height = layout.KeyHeight * height,
                Width = width,
                Visibility = isVisible ? Visibility.Visible : Visibility.Hidden
            };

            if (layout.Style != null)
            {
                var style = GetStyle(layout);
                control.Button.Style = style;
            }

            if (layout.FontSize != 0)
            {
                control.FontSize = _fontSizeMultiplier * layout.FontSize;
            }

            control.KeyboardHost = _host;
            control.SetMultiplier(layout.Multiplier, layout.RepeatMultiplier);

            Canvas.SetLeft(control, left);
            Canvas.SetTop(control, top);

            _canvas.Children.Add(control);

            return control;
        }

        static void FixControl(Control control, NonGapKeyLayout layout)
        {
            if (layout.FontSize != 0)
            {
                control.FontSize = layout.FontSize;
            }
        }

        protected override void CreateGapControl(GapKeyLayout layout, double left, double top, double width, double height)
        {
            if (layout.Style != null)
            {
                var style = GetStyle(layout);
                var control = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Style = style
                };

                Canvas.SetLeft(control, left);
                Canvas.SetTop(control, top);

                _canvas.Children.Add(control);
            }
        }

        protected override UserControl CreateCharacterControl(CharacterKeyLayout layout, double left, double top, double width, double height, bool isVisible)
        {
            var control = CreateControl<AlphanumericKeytop>(layout, left, top, width, height, isVisible);

            control.SendValue = layout.Value;
            control.ShiftSendValue = layout.ShiftValue;
            control.Vocal = layout.Vocal;
            control.ShiftVocal = layout.ShiftVocal;
            control.ShowHints = layout.ShowHints;

            control.ShiftKeytop = layout.ShiftCaption;

            if (layout.IsStrikethrough)
            {
                control.TextDecorations = _strikethrough;
            }

            return control;
        }

        protected override UserControl CreateActionControl(ActionKeyLayout layout, double left, double top, double width, double height, bool isVisible)
        {
            var control = CreateControl<SpecialKeytop>(layout, left, top, width, height, isVisible);

            control.ActionName = layout.Action;
            control.Vocal = layout.Vocal;

            return control;
        }

        protected override UserControl CreateToggleControl(ToggleKeyLayout layout, double left, double top, double width, double height, bool isVisible)
        {
            var control = CreateControl<ToggleKeytop>(layout, left, top, width, height, isVisible);

            control.StateName = layout.StateName;
            control.SetVocal = layout.SetVocal;
            control.UnsetVocal = layout.UnsetVocal;

            return control;
        }

        protected override UserControl CreateStateControl(StateKeyLayout layout, StateShowHideContainer<UserControl> list, double left, double top, double width, double height, bool isVisible)
        {
            var control = CreateControl<StateKeytop>(layout, left, top, width, height, isVisible);

            control.StateName = layout.StateName;
            control.Vocal = layout.Vocal;

            control.TheKeytop.Click += (s, e) => Apply(list);

            return control;
        }

        void Apply(StateShowHideContainer<UserControl> list)
        {
            foreach (var control in list.HideList)
            {
                control.Visibility = Visibility.Collapsed;
            }

            foreach (var control in list.ShowList)
            {
                control.Visibility = Visibility.Visible;
            }
        }
    }
}

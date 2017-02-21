namespace Microsoft.HandsFree.Keyboard.Controls
{
    using Microsoft.HandsFree.Keyboard.Controls.Layout;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for RenderedKeyboard.xaml
    /// </summary>
    public partial class RenderedKeyboard : UserControl
    {
        IKeyboardHost host;

        KeyboardLayout layout;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RenderedKeyboard()
        {
            InitializeComponent();

            SizeChanged += RenderedKeyboard_SizeChanged;
        }

        /// <summary>
        /// Measure the desired size.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size desired;

            if (layout != null && host != null)
            {
                var keyHeight = layout.KeyHeight;
                var keyWidth = layout.KeyWidth;

                if (constraint.Width * keyHeight < constraint.Height * keyWidth)
                {
                    // Constrained by available width.
                    desired = new Size(constraint.Width, constraint.Width * keyHeight / keyWidth);
                }
                else
                {
                    // Constrained by available height.
                    desired = new Size(constraint.Height * keyWidth / keyHeight, constraint.Height);
                }
            }
            else
            {
                desired = new Size(0, 0);
            }

            return desired;
        }

        void RenderedKeyboard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (layout != null && host != null)
            {
                double fontSizeMultiplier = 1;

                FontSize = fontSizeMultiplier * (0 < layout.FontSize ? layout.FontSize : 24);

                TheCanvas.Children.Clear();

                var keyHeight = layout.KeyHeight;
                var keyWidth = layout.KeyWidth;

                var availableKeyHeight = ActualHeight / keyHeight;
                var availableKeyWidth = ActualWidth / keyWidth;

                var keySize = Math.Min(availableKeyHeight, availableKeyWidth);

                var context = new CanvasLayoutContext(host, TheCanvas, layout, TheCanvas.ActualWidth, TheCanvas.ActualHeight, keySize, fontSizeMultiplier);
                context.Run();

                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Load new layout.
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="host"></param>
        public void LoadLayout(KeyboardLayout layout, IKeyboardHost host)
        {
            this.layout = layout;
            this.host = host;

            RenderedKeyboard_SizeChanged(this, null);
        }
    }
}

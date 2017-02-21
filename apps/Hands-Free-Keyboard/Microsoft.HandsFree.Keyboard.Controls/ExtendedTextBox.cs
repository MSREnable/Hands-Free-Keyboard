using Microsoft.HandsFree.Prediction.Api;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Extended text box with additional eye gaze support.
    /// </summary>
    public class ExtendedTextBox : TextBox
    {
        /// <summary>
        /// Shift toggle state property.
        /// </summary>
        public static readonly DependencyProperty ShiftToggleStateProperty = DependencyProperty.Register(nameof(ShiftToggleState), typeof(ToggleState), typeof(ExtendedTextBox),
            new PropertyMetadata(OnToggleStateChanged));

        /// <summary>
        /// Is all selected property.
        /// </summary>
        public static readonly DependencyProperty IsAllSelectedProperty = DependencyProperty.Register(nameof(IsAllSelected), typeof(bool), typeof(ExtendedTextBox),
            new PropertyMetadata(OnIsAllSelectedChanged));

        /// <summary>
        /// Selection start property.
        /// </summary>
        public static readonly DependencyProperty TextSliceProperty = DependencyProperty.Register(nameof(TextSlice), typeof(TextSlice), typeof(ExtendedTextBox),
            new PropertyMetadata(TextSlice.Empty, OnTextSliceChanged));

        /// <summary>
        /// The toggle state corresponding to Shift.
        /// </summary>
        public ToggleState ShiftToggleState { get { return (ToggleState)GetValue(ShiftToggleStateProperty); } set { SetValue(ShiftToggleStateProperty, value); } }

        /// <summary>
        /// Query or force all text to be selected.
        /// </summary>
        public bool IsAllSelected { get { return (bool)GetValue(IsAllSelectedProperty); } set { SetValue(IsAllSelectedProperty, value); } }

        /// <summary>
        /// Start of the selection.
        /// </summary>
        public TextSlice TextSlice { get { return (TextSlice)GetValue(TextSliceProperty); } set { SetValue(TextSliceProperty, value); } }

        string was = string.Empty;

        bool _isUpdatingText;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExtendedTextBox()
        {
            Loaded += (s, e) => Focus();
        }

        void UpdateSelectionCondition()
        {
            if (TextSlice.Start == 0 && TextSlice.Length == Text.Length)
            {
                IsAllSelected = true;

                // When the entire text box is selected, press the shift key.

                var state = ShiftToggleState;

                if (state != null)
                {
                    state.IsChecked = true;
                }
            }
            else
            {
                IsAllSelected = false;
            }
        }

        static void OnTextSliceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (ExtendedTextBox)d;
            box.TextSliceChanged();
        }

        void TextSliceChanged()
        {
            Debug.Assert(!_isUpdatingText);
            _isUpdatingText = true;

            if (Text != TextSlice.Text)
            {
                Text = TextSlice.Text;
            }

            Debug.Assert(_isUpdatingText);
            _isUpdatingText = false;

            if (SelectionStart != TextSlice.Start || SelectionLength != TextSlice.Length)
            {
                Select(TextSlice.Start, TextSlice.Length);
            }
        }

        static void OnToggleStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (ExtendedTextBox)d;

            box.was = string.Empty;
            box.UpdateSelectionCondition();
        }

        static void OnIsAllSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(!object.Equals(e.OldValue, e.NewValue));

            if ((bool)e.NewValue)
            {
                var box = (ExtendedTextBox)d;

                box.SelectAll();
            }
        }

        /// <summary>
        /// Handle selection changed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (!_isUpdatingText)
            {
                TextSlice = new TextSlice(Text, SelectionStart, SelectionLength, CaretIndex != SelectionStart);
            }

            UpdateSelectionCondition();
        }

        /// <summary>
        /// Handle text changing.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            // The text as it is now.
            var now = Text;

            // Find the common prefix for what was and what is now.
            var head = 0;
            while (head < was.Length && head < now.Length && was[head] == now[head])
            {
                head++;
            }

            // Find the common suffix.
            var wasTail = was.Length;
            var nowTail = now.Length;
            while (head < wasTail && head < nowTail && was[wasTail - 1] == now[nowTail - 1])
            {
                wasTail--;
                nowTail--;
            }

            // Extract the deleted text.
            var nowChanged = now.Substring(head, nowTail - head);
            var wasChanged = was.Substring(head, wasTail - head);

            if (nowChanged == string.Empty && wasChanged.Length != 0 && char.IsUpper(wasChanged[0]))
            {
                // If we've inserted nothing and deleted something that starts with a capital, we're
                // can restore the shift key.
                ShiftToggleState.IsChecked = true;
            }
            else if (wasChanged == string.Empty && nowChanged == " " && head != 0 && now[head - 1].IsSentenceEnding())
            {
                // If we've inserted a space and deleted nothing, check whetehr a fullstop precedes
                // the space and if so press the shift key.
                ShiftToggleState.IsChecked = true;
            }

            // What is becomes what was.
            was = now;

            if (!_isUpdatingText)
            {
                TextSlice = new TextSlice(Text, SelectionStart, SelectionLength, SelectionStart != CaretIndex);
            }
        }
    }
}

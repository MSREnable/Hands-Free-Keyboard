namespace Microsoft.HandsFree.Keyboard.Controls
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Keyboard row contrainer control.
    /// </summary>
    public class KeyboardRow : Grid
    {
        /// <summary>
        /// Amount of offset given to row property.
        /// </summary>
        public static readonly DependencyProperty StaggerProperty =
            DependencyProperty.Register("Stagger", typeof(double), typeof(KeyboardRow), new PropertyMetadata(0.0));

        /// <summary>
        /// Constructor.
        /// </summary>
        public KeyboardRow()
        {
            Loaded += (s, e) => RedoGridLayout();
        }

        /// <summary>
        /// Amount of offset before start of row.
        /// </summary>
        public double Stagger
        {
            get { return (double)GetValue(StaggerProperty); }
            set { SetValue(StaggerProperty, value); }
        }

        void AddStarColumn(double stars)
        {
            var columnDefinition = new ColumnDefinition
            {
                Width = new GridLength(stars, GridUnitType.Star)
            };
            ColumnDefinitions.Add(columnDefinition);
        }

        void RedoGridLayout()
        {
            // Get the amount to stagger the row start by.
            var stagger = Stagger;

            // The column to absorb the additional stagger amount.
            var staggerFillColumn = -1;

            // Count the number of column spans and find the index of the column to absorb any
            // excess left by staggering the first column.
            var columnSpans = (int)Math.Ceiling(stagger);
            for (var index = 0; index < Children.Count; index++)
            {
                var child = (DependencyObject)Children[index];

                var columnSpan = (int)child.GetValue(ColumnSpanProperty);
                columnSpans += columnSpan;
            }

            // If no column marked to absorb any excess stagger amount...
            if (staggerFillColumn == -1)
            {
                // ...and the excess to the last column.
                staggerFillColumn = Children.Count - 1;
            }

            // We're going to rebuild to columns.
            ColumnDefinitions.Clear();
            var column = 0;

            // If there's a stagger, emit an initial column.
            if (stagger != 0)
            {
                AddStarColumn(stagger);
                column++;
            }

            // Layout the columns.
            for (var index = 0; index < Children.Count; index++)
            {
                var child = (DependencyObject)Children[index];

                var columnSpan = (int)child.GetValue(ColumnSpanProperty);
                var stars = 1.0;
                if (index == staggerFillColumn)
                {
                    stars += Math.Ceiling(stagger) - stagger;
                }
                AddStarColumn(stars);

                child.SetValue(ColumnProperty, column);
                column++;

                // Add spanned columns.
                for (var span = 1; span < columnSpan; span++)
                {
                    AddStarColumn(1);
                    column++;
                }

            }
        }
    }
}

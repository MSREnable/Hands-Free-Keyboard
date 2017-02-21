using System.Windows;

namespace Microsoft.HandsFree.Keyboard.Controls
{
    /// <summary>
    /// Attached properties to modify standard controls.
    /// </summary>
    public static class HandsFree
    {
        /// <summary>
        /// Mouse down multiplier.
        /// </summary>
        public static DependencyProperty MultiplierProperty = DependencyProperty.RegisterAttached("Multiplier", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(1.0));

        /// <summary>
        /// Repeat mouse down multiplier.
        /// </summary>
        public static DependencyProperty RepeatMultiplierProperty = DependencyProperty.RegisterAttached("RepeatMultiplier", typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(0.0));

        /// <summary>
        /// Set multiplier value.
        /// </summary>
        /// <param name="element">The element to be changed.</param>
        /// <param name="value">The multiplier value.</param>
        public static void SetMultiplier(FrameworkElement element, double value)
        {
            element.SetValue(MultiplierProperty, value);
        }

        /// <summary>
        /// Get multiplier value.
        /// </summary>
        /// <param name="element">The element to be read.</param>
        /// <returns>The multiplier value.</returns>
        public static double GetMultiplier(FrameworkElement element)
        {
            return (double)element.GetValue(MultiplierProperty);
        }

        /// <summary>
        /// Set repeat multiplier value.
        /// </summary>
        /// <param name="element">The element to be changed.</param>
        /// <param name="value">The repeat multiplier value.</param>
        public static void SetRepeatMultiplier(FrameworkElement element, double value)
        {
            element.SetValue(RepeatMultiplierProperty, value);
        }

        /// <summary>
        /// Get repeat multiplier value.
        /// </summary>
        /// <param name="element">The element to be read.</param>
        /// <returns>The repeat multiplier value.</returns>
        public static double GetRepeatMultiplier(FrameworkElement element)
        {
            return (double)element.GetValue(RepeatMultiplierProperty);
        }
    }
}

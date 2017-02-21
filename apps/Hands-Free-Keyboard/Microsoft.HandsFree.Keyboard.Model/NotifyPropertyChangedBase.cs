using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Implementation of INotifyPropertyChanged.
    /// </summary>
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Notify subscripters the property has changed.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);

                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Set field backing a property to a new value.
        /// </summary>
        /// <typeparam name="T">The property/field type.</typeparam>
        /// <param name="field">The field backing the property.</param>
        /// <param name="value">The potential new value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True iff the value changed.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var changed = !object.Equals(field, value);

            if (changed)
            {
                field = value;

                NotifyPropertyChanged(propertyName);
            }

            return changed;
        }

        /// <summary>
        /// Event fired when property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}

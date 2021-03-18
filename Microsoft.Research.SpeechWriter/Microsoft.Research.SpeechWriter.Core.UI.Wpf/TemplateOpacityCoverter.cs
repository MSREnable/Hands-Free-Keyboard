using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Research.SpeechWriter.Core.UI.Wpf
{
    public class TemplateOpacityCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double opacity;

            if (value is InterstitialNonItem)
            {
                opacity = 0.0;
            }
            else if (value is GhostWordItem || value is GhostStopItem)
            {
                opacity = 0.2;
            }
            else
            {
                opacity = 1.0;
            }

            return opacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

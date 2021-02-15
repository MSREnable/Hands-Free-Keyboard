using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using Windows.UI.Xaml.Data;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    public class TemplateOpacityCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

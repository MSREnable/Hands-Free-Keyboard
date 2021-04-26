using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Microsoft.Research.SpeechWriter.DemoAppUwp
{
    public class TemplateTypeConverter : IValueConverter
    {
        private readonly Dictionary<string, DataTemplate> _templates = new Dictionary<string, DataTemplate>();

        internal void LoadTemplates(ResourceDictionary resources)
        {
            foreach (var pair in resources)
            {
                var value = pair.Value as DataTemplate;
                if (value != null)
                {
                    var key = pair.Key as string;

                    if (key != null)
                    {
                        _templates.Add(key, value);
                    }
                }
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = value.GetType();
            var typeName = type.Name;
            _templates.TryGetValue(typeName, out var template);
            return template ?? value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

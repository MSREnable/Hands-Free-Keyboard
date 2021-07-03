using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Research.SpeechWriter.UI.Uwp
{
    public sealed partial class ButtonResourceHelper : UserControl
    {
        private readonly Dictionary<Type, DataTemplate> _templateDictionary = new Dictionary<Type, DataTemplate>();

        public ButtonResourceHelper()
        {
            this.InitializeComponent();
        }

        internal DataTemplate GetTemplate(object value)
        {
            var type = value.GetType();
            if (!_templateDictionary.TryGetValue(type, out var template))
            {
                var typeName = type.Name;

                var resources = (IEnumerable<KeyValuePair<object, object>>)Resources;

                using (var enumerator = resources.GetEnumerator())
                {

                    while (template == null && enumerator.MoveNext())
                    {
                        if (Equals(enumerator.Current.Key, typeName))
                        {
                            template = enumerator.Current.Value as DataTemplate;
                        }
                    }
                }

                _templateDictionary.Add(type, template);
            }

            return template;
        }
    }
}

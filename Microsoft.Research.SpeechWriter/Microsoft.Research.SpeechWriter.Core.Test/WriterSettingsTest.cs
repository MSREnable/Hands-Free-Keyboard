using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    [Parallelizable(ParallelScope.All)]

    class WriterSettingsTest
    {
        private IEnumerable<WriterSettingName> Names
        {
            get
            {
                foreach (WriterSettingName name in Enum.GetValues(typeof(WriterSettingName)))
                {
                    yield return name;
                }
            }
        }

        [Test]
        public void GetSetTest()
        {
            var settings = new WriterSettings();

            foreach (var trueName in Names)
            {
                foreach (var name in Names)
                {
                    settings.Set(name, name == trueName);
                }

                foreach (var name in Names)
                {
                    var expected = name == trueName;
                    var actual = settings.Get(name);
                    if (IsButton(trueName) == IsButton(name) || !IsButton(name))
                    {
                        Assert.AreEqual(expected, actual);
                    }
                    else
                    {
                        Assert.IsTrue(IsButton(name));

                        var count = (settings.Get(WriterSettingName.SmallButtons) ? 1 : 0) +
                            (settings.Get(WriterSettingName.MediumButtons) ? 1 : 0) +
                            (settings.Get(WriterSettingName.LargeButtons) ? 1 : 0);
                        Assert.AreEqual(1, count);
                    }
                }
            }

            bool IsButton(WriterSettingName name) =>
                name == WriterSettingName.SmallButtons ||
                name == WriterSettingName.MediumButtons ||
                name == WriterSettingName.LargeButtons;
        }

        [Test]
        public void AllNamesNamedTest()
        {
            var nameSet = new HashSet<WriterSettingName>(Names);

            var type = typeof(WriterSettings);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                if (property.PropertyType == typeof(bool))
                {
                    var parsedName = Enum.Parse<WriterSettingName>(propertyName);
                    Assert.IsTrue(nameSet.Contains(parsedName), "Name is in settings", propertyName);
                    nameSet.Remove(parsedName);
                }
                else
                {
                    Assert.AreEqual(typeof(double), property.PropertyType);
                    Assert.AreEqual(nameof(WriterSettings.ButtonScale), property.Name);
                }
            }

            Assert.IsTrue(nameSet.Count == 0, "All names accounted for", nameSet);
        }
    }
}

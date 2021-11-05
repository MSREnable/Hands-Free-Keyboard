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
                    Assert.AreEqual(expected, actual);
                }
            }
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
                var parsedName = Enum.Parse<WriterSettingName>(propertyName);
                Assert.IsTrue(nameSet.Contains(parsedName), "Name is in settings", propertyName);
                nameSet.Remove(parsedName);
            }

            Assert.IsTrue(nameSet.Count == 0, "All names accounted for", nameSet);
        }
    }
}

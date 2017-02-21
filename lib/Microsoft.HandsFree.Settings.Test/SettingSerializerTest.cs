using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Settings.Serialization;
using Microsoft.HandsFree.Settings.Test.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Settings.Test
{
    [TestClass]
    public class SettingSerializerTest
    {
        [TestMethod]
        public void ExerciseCreation()
        {
            var serializerFirst = SettingSerializer.GetSerializer<Sample>();
            var serializerSecond = SettingSerializer.GetSerializer(typeof(Sample));
            Assert.AreSame(serializerFirst, serializerSecond, "Both functions should return same object");

            // ValidationContext.Validate(serializerFirst);
        }

        [TestMethod]
        public void KeyboardSettingsTest()
        {
            var serializer = SettingSerializer.GetSerializer<AppSettings>();
            Assert.IsNotNull(serializer, "Should get a result");

            // ValidationContext.Validate(serializer);
        }
    }
}

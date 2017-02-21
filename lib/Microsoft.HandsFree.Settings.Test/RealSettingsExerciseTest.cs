using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Settings.Test
{
    [TestClass]
    public class RealSettingsExerciseTest
    {
        static readonly string SettingsPath = SettingsDirectory.GetDefaultSettingsFilePath("HandsFree.Keyboard.config");

        [TestMethod]
        public void ReadSettingsSerialized()
        {
            var fileText = File.ReadAllText(SettingsPath);
            var settings = SettingsSerializer.FromXmlString<AppSettings>(fileText);

            var xml = SettingsSerializer.ToXmlString(settings, "Settings");
        }

        [TestMethod]
        public void ReadSettingsXmlDocument()
        {
            var fileText = File.ReadAllText(SettingsPath);

            var store = SettingsStore<AppSettings>.CreateFromXml(fileText);
            var rejigged = store.ToXmlString();
        }

    }
}

using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Sensors;
using Microsoft.HandsFree.Settings.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace Microsoft.HandsFree.Settings.Test
{

    /// <summary>
    /// Summary description for GeneralSettingsTest
    /// </summary>
    [TestClass]
    public class GeneralSettingsTest
    {
        static void CreateSaveLoadGeneric<T>()
            where T : INotifyPropertyChanged, new()
        {
            var createdOb = SettingsSerializer.CreateDefault<T>();
            var savedXmlString = SettingsSerializer.ToXmlString(createdOb, "Settings");
            var loadedOb = SettingsSerializer.FromXmlString<T>(savedXmlString);
        }

        [TestMethod]
        public void CreateSaveLoadSpecific()
        {
            var createdOb = SettingsSerializer.CreateDefault<GeneralSettings>();
            var savedXmlString = SettingsSerializer.ToXmlString(createdOb, "Settings");
            var loadedOb = SettingsSerializer.FromXmlString<GeneralSettings>(savedXmlString);
        }

        [TestMethod]
        public void CreateSaveLoadGeneral()
        {
            CreateSaveLoadGeneric<GeneralSettings>();
        }

        [TestMethod]
        public void CreateSaveLoadFilter()
        {
            CreateSaveLoadGeneric<Filters.Settings>();
        }

        [TestMethod]
        public void CreateSaveLoadKeyboard()
        {
            CreateSaveLoadGeneric<KeyboardSettings>();
        }

        [TestMethod]
        public void CreateSaveLoadLogging()
        {
            CreateSaveLoadGeneric<LoggingSettings>();
        }

        [TestMethod]
        public void CreateSaveLoadPrediction()
        {
            CreateSaveLoadGeneric<PredictionSettings>();
        }

        [TestMethod]
        public void CreateSaveLoadEverything()
        {
            CreateSaveLoadGeneric<AppSettings>();
        }
    }
}

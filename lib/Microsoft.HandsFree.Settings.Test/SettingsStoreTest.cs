using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Settings.Test
{
    [TestClass]
    public class SettingsStoreTest
    {
        [TestMethod]
        public async Task LoadFollowedByExternalUpdate()
        {
            var path = Path.GetTempFileName();

            try
            {
                File.WriteAllLines(path, new[]
                {
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                    "<HandsFreeSettings>",
                    "  <General VoiceVolume=\"7\"/>",
                    "</HandsFreeSettings>"
                });

                using (var store = SettingsStore<AppSettings>.Create(path))
                {
                    Assert.AreEqual(7, store.Settings.General.VoiceVolume);
                    Assert.AreEqual(10, store.Settings.General.ClickVolume);
                    Assert.AreEqual(15, store.Settings.Mouse.Filter.HistoryLength);

                    var semaphore = new SemaphoreSlim(0);
                    store.Changed += (s, e) =>
                        {
                            semaphore.Release();
                        };

                    File.WriteAllLines(path, new[]
                    {
                        "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                        "<HandsFreeSettings>",
                        "  <General ClickVolume=\"3\"/>",
                        "</HandsFreeSettings>"
                    });

                    Assert.IsTrue(await semaphore.WaitAsync(TimeSpan.FromSeconds(0.5)));
                    while (await semaphore.WaitAsync(TimeSpan.FromSeconds(0.25)))
                    {
                        // Spin.
                    }

                    Assert.AreEqual(7, store.Settings.General.VoiceVolume);
                    Assert.AreEqual(3, store.Settings.General.ClickVolume);
                    Assert.AreEqual(15, store.Settings.Mouse.Filter.HistoryLength);
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void CheckFirstInstallBehaviour()
        {
            // Get a file to work with.
            var path = Path.GetTempFileName();
            Assert.IsTrue(File.Exists(path));
            File.Delete(path);
            Assert.IsFalse(File.Exists(path));

            try
            {
                Assert.IsFalse(File.Exists(path), "File does not initially exist");

                using (var store = SettingsStore<AppSettings>.Create(path))
                {
                    Assert.AreEqual(10, store.Settings.General.ClickVolume, "ClickVolume starts at known default value");
                    store.Settings.General.ClickVolume = 5;
                    Assert.AreEqual(5, store.Settings.General.ClickVolume, "Change happened");
                    store.Save();
                    Assert.AreEqual(5, store.Settings.General.ClickVolume, "Locally set ClickVolume change retained");
                }

                Assert.IsTrue(File.Exists(path), "File was created by the above");

                using (var store = SettingsStore<AppSettings>.Create(path))
                {
                    Assert.AreEqual(5, store.Settings.General.ClickVolume, "ClickVolume change persisted");
                    store.Reset();
                    Assert.AreEqual(10, store.Settings.General.ClickVolume, "ClickVolume returned to default");
                }
            }
            finally
            {
                // Clean up test.
                File.Delete(path);
            }
        }
    }
}

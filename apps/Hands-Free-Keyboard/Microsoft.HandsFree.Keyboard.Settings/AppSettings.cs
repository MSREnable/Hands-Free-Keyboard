using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings;
using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// 
    /// </summary>
    public class AppSettings : NotifyingObject
    {
        static readonly SettingsStore<AppSettings> store = LoadSettings();

        /// <summary>
        /// 
        /// </summary>
        public static readonly AppSettings Instance = store.Settings;

        /// <summary>
        /// 
        /// </summary>
        public static readonly SettingsStore<AppSettings> Store = store;

        static SettingsStore<AppSettings> LoadSettings()
        {
            var path = SettingsDirectory.GetDefaultSettingsFilePath(AppSettings.DefaultSettingsFile);
            var store = SettingsStore<AppSettings>.Create(path);

            return store;
        }

        /// <summary>
        /// 
        /// </summary>
        public GeneralSettings General { get { return _general; } set { SetProperty(ref _general, value); } }
        GeneralSettings _general;

        /// <summary>
        /// 
        /// </summary>
        public KeyboardSettings Keyboard { get { return _keyboard; } set { SetProperty(ref _keyboard, value); } }
        KeyboardSettings _keyboard;

        /// <summary>
        /// 
        /// </summary>
        public PublicNarrationSettings PublicNarration { get { return _publicNarration; } set { SetProperty(ref _publicNarration, value); } }
        PublicNarrationSettings _publicNarration;

        /// <summary>
        /// 
        /// </summary>
        public PrivateNarrationSettings PrivateNarration { get { return _privateNarration; } set { SetProperty(ref _privateNarration, value); } }
        PrivateNarrationSettings _privateNarration;

        /// <summary>
        /// 
        /// </summary>
        public PredictionSettings Prediction { get { return _prediction; } set { SetProperty(ref _prediction, value); } }
        PredictionSettings _prediction;

        /// <summary>
        /// 
        /// </summary>
        public Mouse.Settings Mouse { get { return _mouse; } set { SetProperty(ref _mouse, value); } }
        Mouse.Settings _mouse;

        /// <summary>
        /// 
        /// </summary>
        public static string DefaultSettingsFile => SettingsDirectory.GetDefaultSettingsFilePath("HandsFree.Keyboard.config");

        /// <summary>
        /// 
        /// </summary>
        public AppSettings()
        {
            General = new GeneralSettings();
            Keyboard = new KeyboardSettings();
            PublicNarration = new PublicNarrationSettings();
            PrivateNarration = new PrivateNarrationSettings();
            Prediction = new PredictionSettings();
            Mouse = new Mouse.Settings();
        }
    }
}

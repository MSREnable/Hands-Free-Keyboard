using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings.Serialization;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Settings
{
    /// <summary>
    /// 
    /// </summary>
    public class GeneralSettings : NotifyingObject
    {
        int _voiceVolume = 10;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute]
        [SettingDescription("Voice Volume", 1, 10)]
        public int VoiceVolume
        {
            get { return _voiceVolume; }
            set { SetProperty(ref _voiceVolume, value); }
        }

        int _clickVolume = 10;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute]
        [SettingDescription("Click Volume", 0, 10)]
        public int ClickVolume
        {
            get { return _clickVolume; }
            set { SetProperty(ref _clickVolume, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public const string UnsetSlackHostUri = "";

        string _slackHostUri = UnsetSlackHostUri;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute]
        [DefaultValue(UnsetSlackHostUri)]
        [SettingDescription]
        public string SlackHostUri
        {
            get { return _slackHostUri; }
            set { SetProperty(ref _slackHostUri, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public const string DefaultSlackChannel = "#general";

        string _slackChannel = DefaultSlackChannel;
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute]
        [DefaultValue("DefaultSlackChannel")]
        [SettingDescription]
        public string SlackChannel
        {
            get { return _slackChannel; }
            set { SetProperty(ref _slackChannel, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        private const string UpdateUrlFormat = "http://msrenable.blob.core.windows.net/install/EnablingTech-{0}/latest/Microsoft.HandsFree.Keyboard";

        /// <summary>
        ///
        /// </summary>
        [XmlIgnore]
        public string UpdateUrl
        {
            get
            {
                var branch = ((AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute))).InformationalVersion;
                return string.Format(UpdateUrlFormat, branch);
            }
        }

        /// <summary>
        /// Left margin for display (to allow for docked toolbars).
        /// </summary>
        [SettingDescription("Left Margin", 0, 1000, 10)]
        public int LeftMargin { get { return _leftMargin; } set { if (SetProperty(ref _leftMargin, value)) { OnPropertyChanged(nameof(Margins)); } } }
        int _leftMargin;

        /// <summary>
        /// Top margin for display (to allow for docked toolbars).
        /// </summary>
        [SettingDescription("Top Margin", 0, 1000, 10)]
        public int TopMargin { get { return _topMargin; } set { if (SetProperty(ref _topMargin, value)) { OnPropertyChanged(nameof(Margins)); } } }
        int _topMargin;

        /// <summary>
        /// Right margin for display (to allow for docked toolbars).
        /// </summary>
        [SettingDescription("Right Margin", 0, 1000, 10)]
        public int RightMargin { get { return _rightMargin; } set { if (SetProperty(ref _rightMargin, value)) { OnPropertyChanged(nameof(Margins)); } } }
        int _rightMargin;

        /// <summary>
        /// Bottom margin for display (to allow for docked toolbars).
        /// </summary>
        [SettingDescription("Bottom Margin", 0, 1000, 10)]
        public int BottomMargin { get { return _bottomMargin; } set { if (SetProperty(ref _bottomMargin, value)) { OnPropertyChanged(nameof(Margins)); } } }
        int _bottomMargin;

        /// <summary>
        /// The margins around the display Viewbox.
        /// </summary>
        [XmlIgnore]
        public Thickness Margins { get { return new Thickness(LeftMargin, TopMargin, RightMargin, BottomMargin); } }
    }
}

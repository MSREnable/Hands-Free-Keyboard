using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Narration settings for user feedback.
    /// </summary>
    public class PrivateNarrationSettings : NarrationSettings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PrivateNarrationSettings()
            : base(sentenceBehaviorDefault: SentenceBehavior.Command,
                  readCompletedWordsDefault: false,
                  readKeyTopsDefault: false,
                  isClickOnDefault: true,
                  playVocalGesturesDefault: false,
                  deviceDescription: "Click Audio Device")
        {
        }

        /// <summary>
        /// Output device.
        /// </summary>
        [SettingDescription("Click Audio Device", SettingNudgerFactoryBehavior.NameAssociatedMember)]
        public override string Device { get { return _device; } set { SetProperty(ref _device, value); } }
        string _device = string.Empty;
    }
}

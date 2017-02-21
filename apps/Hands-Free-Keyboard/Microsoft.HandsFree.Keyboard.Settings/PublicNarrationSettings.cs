using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Settings for audience narration.
    /// </summary>
    public class PublicNarrationSettings : NarrationSettings
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PublicNarrationSettings()
            : base(sentenceBehaviorDefault: SentenceBehavior.Always,
                  readCompletedWordsDefault: false,
                  readKeyTopsDefault: false,
                  isClickOnDefault: false,
                  playVocalGesturesDefault: true,
                  deviceDescription: "Sentence Speaking Audio Device")
        {
        }

        /// <summary>
        /// Output device.
        /// </summary>
        [SettingDescription("Sentence Speaking Audio Device", SettingNudgerFactoryBehavior.NameAssociatedMember)]
        public override string Device { get { return _device; } set { SetProperty(ref _device, value); } }
        string _device = string.Empty;
    }
}

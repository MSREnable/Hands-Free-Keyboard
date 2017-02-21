using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Settings;
using System.Collections.Generic;
using System.Speech.Synthesis;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Settings.Nudgers;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    class TextToAudioProviderFactory : ITextToAudioProviderFactory
    {
        class VoiceDynamicValueSetting : DynamicValueSetting
        {
            internal string Name { get; set; }
        }

        TextToAudioProviderFactory()
        {
        }

        internal static readonly ITextToAudioProviderFactory Instance = new TextToAudioProviderFactory();

        ITextToAudioProvider ITextToAudioProviderFactory.Create(INarrationSettings settings)
        {
            var provider = new TextToAudioProvider(settings);
            return provider;
        }

        internal static void UpdateSettings()
        {
            var dynamicVoices = new List<VoiceDynamicValueSetting>();

            using (var synthesizer = new SpeechSynthesizer())
            {
                var defaultVoice = synthesizer.Voice.Name;
                dynamicVoices.Add(new VoiceDynamicValueSetting { Key = string.Empty, ValueString = defaultVoice, Name = synthesizer.Voice.Name });

                foreach (var installed in synthesizer.GetInstalledVoices())
                {
                    if (installed.Enabled && installed.VoiceInfo.Name != defaultVoice)
                    {
                        var voice = new VoiceDynamicValueSetting { Key = installed.VoiceInfo.Name, ValueString = installed.VoiceInfo.Name, Name = synthesizer.Voice.Name };
                        dynamicVoices.Add(voice);
                    }
                }
            }

            AppSettings.Instance.PublicNarration.SentenceVoicingNudger.Values = dynamicVoices.ToArray();
            AppSettings.Instance.PrivateNarration.SentenceVoicingNudger.Values = dynamicVoices.ToArray();

            AppSettings.Instance.PublicNarration.WordVoicingNudger.Values = dynamicVoices.ToArray();
            AppSettings.Instance.PrivateNarration.WordVoicingNudger.Values = dynamicVoices.ToArray();

            AppSettings.Instance.PublicNarration.LetterVoicingNudger.Values = dynamicVoices.ToArray();
            AppSettings.Instance.PrivateNarration.LetterVoicingNudger.Values = dynamicVoices.ToArray();
        }
    }
}

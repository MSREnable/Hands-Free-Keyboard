using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Keyboard.Settings;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Xml;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    internal class TextToAudioProvider : ITextToAudioProvider
    {
        readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        readonly INarrationSettings _settings;

        readonly string _voiceDefault;

        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        internal TextToAudioProvider(INarrationSettings settings)
        {
            _voiceDefault = _synthesizer.Voice.Name;

            _settings = settings;
        }

        string GetVoiceName(Voice voice)
        {
            string voiceName;
            switch (voice)
            {
                default:
                case Voice.Sentence:
                    voiceName = _settings.SentenceVoicing;
                    break;

                case Voice.Word:
                    voiceName = _settings.WordVoicing;
                    break;

                case Voice.Letter:
                    voiceName = _settings.LetterVoicing;
                    break;
            }

            return voiceName != string.Empty ? voiceName : _voiceDefault;
        }

        public AudioTheme GetAudioTheme(Voice voice)
        {
            string voiceName = GetVoiceName(voice);

            foreach (var v in _synthesizer.GetInstalledVoices().Select(v => v.VoiceInfo))
            {
                if (v.Name == voiceName)
                {
                    return v.Gender == VoiceGender.Female ? AudioTheme.Female : AudioTheme.Male;
                }
            }

            return AudioTheme.Male;
        }

        byte[] ITextToAudioProvider.ToAudio(Voice voice, string text)
        {
            var name = GetVoiceName(voice);

            var rate = voice == Voice.Word ? 1.0 / _settings.WordVoicingRate : 1;

            var volume = 10.0 * AppSettings.Instance.General.VoiceVolume;

            var builder = new PromptBuilder();

            var document = new XmlDocument();

            var voiceElement = document.CreateElement("voice");

            var nameAttribute = document.CreateAttribute("name");
            nameAttribute.Value = name;
            voiceElement.Attributes.Append(nameAttribute);

            var prosodyElement = document.CreateElement("prosody");
            voiceElement.AppendChild(prosodyElement);

            var rateAttribute = document.CreateAttribute("rate");
            rateAttribute.Value = rate.ToString(CultureInfo.InvariantCulture);
            prosodyElement.Attributes.Append(rateAttribute);

            var volumeAttribute = document.CreateAttribute("volume");
            volumeAttribute.Value = volume.ToString(CultureInfo.InvariantCulture);
            prosodyElement.Attributes.Append(volumeAttribute);

            prosodyElement.InnerText = text;

            //builder.AppendSsmlMarkup($"<voice name=\"{name}\"><prosody rate=\"{rate}\" volume=\"{volume}\">{text}</prosody></voice>");
            builder.AppendSsmlMarkup(voiceElement.OuterXml);

            var stream = new MemoryStream();
            _semaphore.Wait();
            _synthesizer.SetOutputToWaveStream(stream);
            _synthesizer.Speak(builder);
            _semaphore.Release();
            var buffer = stream.ToArray();

            return buffer;
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                _synthesizer.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}

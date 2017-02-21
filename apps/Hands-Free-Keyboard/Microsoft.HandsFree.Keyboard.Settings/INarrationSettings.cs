using System.ComponentModel;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Settings for a public and private narration.
    /// </summary>
    public interface INarrationSettings : INotifyPropertyChanged
    {
        /// <summary>
        /// Name of the device to use.
        /// </summary>
        string Device { get; }

        /// <summary>
        /// Should sentences be said or should the speech command just be announced.
        /// </summary>
        SentenceBehavior SentenceBehavior { get; }

        /// <summary>
        /// Behavior of Play button while currently speaking.
        /// </summary>
        SpeechConcurrencyBehavior SpeechConcurrencyBehavior { get; }

        /// <summary>
        /// Number of seconds to elapse before partial sentence recap deemed necessary.
        /// </summary>
        int SentenceRecapThreshold { get; }

        /// <summary>
        /// The voice to use for sentences.
        /// </summary>
        string SentenceVoicing { get; }

        /// <summary>
        /// Should completed words be read.
        /// </summary>
        bool ReadCompletedWords { get; }

        /// <summary>
        /// The voice to use for words.
        /// </summary>
        string WordVoicing { get; }

        /// <summary>
        /// The rate at which words should be spoken.
        /// </summary>
        int WordVoicingRate { get; }

        /// <summary>
        /// Number of ms to wait before starting to fill silence.
        /// </summary>
        int SilenceFillerDelay { get; }

        /// <summary>
        /// Mechanism for filling silence.
        /// </summary>
        SilenceFiller SilenceFiller { get; }

        /// <summary>
        /// Percentage volume of silence filler.
        /// </summary>
        int SilenceFillerVolume { get; }

        /// <summary>
        /// Should individual key tops be read.
        /// </summary>
        bool ReadKeyTops { get; }

        /// <summary>
        /// Read the top suggestion.
        /// </summary>
        bool ReadTopSuggestion { get; }

        /// <summary>
        /// Should letters be read like a cheerleader?
        /// </summary>
        bool IsCheerleaderMode { get; }

        /// <summary>
        /// The voice to use for letters.
        /// </summary>
        string LetterVoicing { get; }

        /// <summary>
        /// Make a click sound if no other sound.
        /// </summary>
        bool IsClickOn { get; }

        /// <summary>
        /// Play the vocal gestures like laughing, coughing, ugh, and argh. If false, only a click will be played
        /// </summary>
        bool PlayVocalGestures { get; }

        /// <summary>
        /// Should we play sound effects.
        /// </summary>
        bool PlaySoundEffects { get; }
    }
}

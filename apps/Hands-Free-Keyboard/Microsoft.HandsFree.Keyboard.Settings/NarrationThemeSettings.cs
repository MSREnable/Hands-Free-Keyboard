using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    class NarrationThemeSettings
    {
        readonly SentenceBehavior _sentenceBehavior;
        readonly bool _readCompletedWords;
        readonly int _silenceFillerDelay;
        readonly SilenceFiller _silenceFiller;
        readonly bool _readKeyTops;
        readonly bool _isCheerleaderMode;
        readonly bool _isClickOn;
        readonly bool _playSoundEffects;

        static Dictionary<NarrationTheme, NarrationThemeSettings> _themes = CreateThemes();

        NarrationThemeSettings(SentenceBehavior sentenceBehavior,
            bool readCompletedWords,
            int silenceFillerDelay,
            SilenceFiller silenceFiller,
            bool readKeyTops,
            bool isCheerleaderMode,
            bool isClickOn,
            bool playSoundEffects)
        {
            _sentenceBehavior = sentenceBehavior;
            _readCompletedWords = readCompletedWords;
            _silenceFillerDelay = silenceFillerDelay;
            _silenceFiller = silenceFiller;
            _readKeyTops = readKeyTops;
            _isCheerleaderMode = isCheerleaderMode;
            _isClickOn = isClickOn;
            _playSoundEffects = playSoundEffects;
        }

        static Dictionary<NarrationTheme, NarrationThemeSettings> CreateThemes()
        {
            var themes = new Dictionary<NarrationTheme, NarrationThemeSettings>();

            themes.Add(NarrationTheme.Original, new NarrationThemeSettings(sentenceBehavior: SentenceBehavior.Always, readCompletedWords: false, silenceFillerDelay: 0, silenceFiller: SilenceFiller.None, readKeyTops: false, isCheerleaderMode: false, isClickOn: true, playSoundEffects: false));
            themes.Add(NarrationTheme.Demo, new NarrationThemeSettings(sentenceBehavior: SentenceBehavior.Always, readCompletedWords: true, silenceFillerDelay: 0, silenceFiller: SilenceFiller.Echo, readKeyTops: true, isCheerleaderMode: false, isClickOn: false, playSoundEffects: true));
            themes.Add(NarrationTheme.PrivateFeedback, new NarrationThemeSettings(sentenceBehavior: SentenceBehavior.Command, readCompletedWords: true, silenceFillerDelay: 0, silenceFiller: SilenceFiller.None, readKeyTops: true, isCheerleaderMode: false, isClickOn: false, playSoundEffects: false));
            themes.Add(NarrationTheme.SimplePublic, new NarrationThemeSettings(sentenceBehavior: SentenceBehavior.OnlyRepetition, readCompletedWords: true, silenceFillerDelay: 0, silenceFiller: SilenceFiller.None, readKeyTops: false, isCheerleaderMode: false, isClickOn: false, playSoundEffects: true));
            themes.Add(NarrationTheme.EchoFilledPublic, new NarrationThemeSettings(sentenceBehavior: SentenceBehavior.Always, readCompletedWords: true, silenceFillerDelay: 0, silenceFiller: SilenceFiller.Echo, readKeyTops: false, isCheerleaderMode: false, isClickOn: false, playSoundEffects: true));

            Debug.Assert(Equals(Enum.GetValues(typeof(NarrationTheme)).GetValue(0), (object)NarrationTheme.Custom),
                "Custom must be the first theme");
            foreach (NarrationTheme theme in Enum.GetValues(typeof(NarrationTheme)))
            {
                Debug.Assert((theme != NarrationTheme.Custom) == themes.ContainsKey(theme),
                    "All themes except Custom must be present in dictionary");
            }

            return themes;
        }

        internal static void SetIndicator(NarrationSettings settings)
        {
            using (var enumerator = _themes.GetEnumerator())
            {
                var theme = NarrationTheme.Custom;

                while (theme == NarrationTheme.Custom && enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current.Value.IsMatch(settings))
                    {
                        theme = current.Key;
                    }
                }

                settings.NarrationTheme = theme;
            }
        }

        bool IsMatch(NarrationSettings settings)
        {
            return settings.SentenceBehavior == _sentenceBehavior &&
                settings.ReadCompletedWords == _readCompletedWords &&
                settings.SilenceFillerDelay == _silenceFillerDelay &&
                settings.SilenceFiller == _silenceFiller &&
                settings.ReadKeyTops == _readKeyTops &&
                settings.IsCheerleaderMode == _isCheerleaderMode &&
                settings.IsClickOn == _isClickOn &&
                settings.PlaySoundEffects == _playSoundEffects;
        }

        internal static void SetValues(NarrationSettings settings)
        {
            NarrationThemeSettings theme;
            if (_themes.TryGetValue(settings.NarrationTheme, out theme))
            {
                theme.Set(settings);
            }
        }

        void Set(NarrationSettings settings)
        {
            settings.SentenceBehavior = _sentenceBehavior;
            settings.ReadCompletedWords = _readCompletedWords;
            settings.SilenceFillerDelay = _silenceFillerDelay;
            settings.SilenceFiller = _silenceFiller;
            settings.ReadKeyTops = _readKeyTops;
            settings.IsCheerleaderMode = _isCheerleaderMode;
            settings.IsClickOn = _isClickOn;
            settings.PlaySoundEffects = _playSoundEffects;
        }
    }
}

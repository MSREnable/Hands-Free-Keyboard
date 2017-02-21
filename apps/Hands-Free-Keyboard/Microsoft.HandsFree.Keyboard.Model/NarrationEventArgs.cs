using Microsoft.HandsFree.Keyboard.Controls;
using System;

namespace Microsoft.HandsFree.Keyboard.Model
{
    public class NarrationEventArgs : EventArgs
    {
        NarrationEventArgs(string keyTop, NarrationEventType eventType, string utterance, int cursorPosition, bool isRepeat, string completedWord)
        {
            KeyTop = keyTop;

            EventType = eventType;

            Utterance = utterance;
            CursorPosition = cursorPosition;

            IsRepeat = isRepeat;

            CompletedWord = completedWord;
        }

        internal static NarrationEventArgs Create(object key, NarrationEventType eventType, string utterance, int cursorPosition, bool isRepeat, string completedWord)
        {
            var special = key as SpecialKeytop;
            var vocal = special != null ? (special.Vocal ?? special.Keytop) : key as string;
            var keyTop = vocal ?? "wibble";

            var args = new NarrationEventArgs(keyTop, eventType, utterance, cursorPosition, isRepeat, completedWord);
            return args;
        }

        internal static NarrationEventArgs Create(NarrationEventArgs argsBase, string suggestion)
        {
            var args = new NarrationEventArgs(argsBase.KeyTop, NarrationEventType.GotSuggestion, argsBase.Utterance, argsBase.CursorPosition, argsBase.IsRepeat, argsBase.CompletedWord)
            {
                Suggestion = suggestion
            };
            return args;
        }

        internal static NarrationEventArgs Create(AudioGesture gesture)
        {
            var args = new NarrationEventArgs(null, NarrationEventType.VocalGesture, null, 0, false, null)
            {
                Gesture = gesture
            };
            return args;
        }

        public AudioGesture Gesture { get; private set; }

        public string KeyTop { get; private set; }

        public NarrationEventType EventType { get; private set; }

        public string CompletedWord { get; private set; }

        public int CursorPosition { get; private set; }

        public string Utterance { get; private set; }

        public bool IsRepeat { get; private set; }

        public string Suggestion { get; private set; }
    }
}

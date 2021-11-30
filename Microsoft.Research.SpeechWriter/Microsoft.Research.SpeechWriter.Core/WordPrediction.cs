using System;

namespace Microsoft.Research.SpeechWriter.Core
{
    class WordPrediction
    {
        internal WordPrediction(int[] score, int index, string rawText, string text, bool isFollowOnFirstWord)
        {
            Score = new int[score.Length];
            Array.Copy(score, Score, score.Length);
            Index = index;
            RawText = rawText;
            Text = text;
            IsFollowOnFirstWord = isFollowOnFirstWord;
        }

        internal int[] Score { get; }
        internal int Index { get; }
        internal string RawText { get; }
        internal string Text { get; }
        internal bool IsFollowOnFirstWord { get; }

        internal int Token => Score[0];

        public override string ToString()
        {
            var scoreString = string.Join("-", Score);
            return $"{Index} {Text} {scoreString}";
        }
    }
}

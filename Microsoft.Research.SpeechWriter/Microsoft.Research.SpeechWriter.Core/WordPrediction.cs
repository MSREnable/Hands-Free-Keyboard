using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    class WordPrediction
    {
        internal WordPrediction(int[] score, int index, string text)
        {
            Score = new int[score.Length];
            Array.Copy(score, Score, score.Length);
            Index = index;
            Text = text;
        }

        internal int[] Score { get; }
        internal int Index { get; }
        internal string Text { get; }

        internal int Token => Score[0];

        public override string ToString()
        {
            var scoreString = string.Join("-", Score);
            return $"{Index} {Text} {scoreString}";
        }
    }
}

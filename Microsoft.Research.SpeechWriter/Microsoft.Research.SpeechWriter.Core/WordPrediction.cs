﻿namespace Microsoft.Research.SpeechWriter.Core
{
    class WordPrediction
    {
        internal WordPrediction(Score score, int index, string rawText, string text, bool isFollowOnFirstWord)
        {
            Score = score;
            Index = index;
            RawText = rawText;
            Text = text;
            IsFollowOnFirstWord = isFollowOnFirstWord;
        }

        internal Score Score { get; }
        internal int Index { get; }
        internal string RawText { get; }
        internal string Text { get; }
        internal bool IsFollowOnFirstWord { get; }

        internal int Token => Score.Token;

        public override string ToString()
        {
            var scoreString = string.Join("-", Score);
            return $"{Index} {Text} {scoreString}";
        }
    }
}

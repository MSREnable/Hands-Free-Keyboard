using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class ScoreReverseComparer : IComparer<Score>
    {
        private ScoreReverseComparer()
        {
        }

        internal static IComparer<Score> Instance { get; } = new ScoreReverseComparer();

        int IComparer<Score>.Compare(Score x, Score y)
        {
            var value = y.CompareTo(x);

            return value;
        }
    }
}

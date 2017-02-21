using System;
namespace Microsoft.HandsFree.Prediction.Historic
{
    /// <summary>
    /// Score for an individual word.
    /// </summary>
    public class KeyScorePair
    {
        /// <summary>
        /// The word associated with the score.
        /// </summary>
        public int key;

        /// <summary>
        /// The score for the word.
        /// </summary>
        public double score;
    }
}

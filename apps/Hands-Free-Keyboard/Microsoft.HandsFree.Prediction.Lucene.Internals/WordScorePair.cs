namespace Microsoft.HandsFree.Prediction.Lucene.Internals
{
    public class WordScorePair
    {
        public WordScorePair(string word, double score)
        {
            Word = word;
            Score = score;
        }

        public string Word { get; private set; }

        public double Score { get; private set; }
    }
}

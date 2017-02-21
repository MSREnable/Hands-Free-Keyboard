using Microsoft.HandsFree.Prediction.Api;
using System;
using System.IO;

namespace Microsoft.HandsFree.Prediction.Engine
{
    class LuceneWordSuggestion : IPredictionSuggestion
    {
        static readonly string LocalApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        static readonly string DirectoryPath = Path.Combine(LocalApplicationDataPath, "Microsoft", "Microsoft Research Enable");
        static readonly string FilePath = Path.Combine(DirectoryPath, "accepted.txt");

        readonly IPredictionEnvironment environment;
        readonly string rawSuggestion;
        readonly int replacementStart;
        readonly int replacementLength;

        public LuceneWordSuggestion(IPredictionEnvironment environment, string rawSuggestion, int replacementStart, int replacementLength)
        {
            this.environment = environment;
            this.rawSuggestion = rawSuggestion;
            this.replacementStart = replacementStart;
            this.replacementLength = replacementLength;
        }

        public double Confidence
        {
            get { return 0.5; }
        }

        public string Text
        {
            get { return rawSuggestion; }
        }

        public int ReplacementStart
        {
            get { return replacementStart; }
        }

        public int ReplacementLength
        {
            get { return replacementLength; }
        }

        public bool CompleteWord
        {
            get { return true; }
        }

        public void Accepted(int index)
        {
            environment.RecordAcceptedSuggestion(index, ReplacementLength, Text);
        }
    }
}


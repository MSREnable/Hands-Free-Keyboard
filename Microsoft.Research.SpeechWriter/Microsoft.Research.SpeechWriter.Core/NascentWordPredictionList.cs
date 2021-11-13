using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    class NascentWordPredictionList
    {
        internal WordPrediction _core;
        internal readonly List<WordPrediction> _compound;
        internal WordPrediction _followOn;

        internal NascentWordPredictionList(WordPrediction prediction, WordPrediction followOn)
        {
            _core = prediction;
            _compound = new List<WordPrediction>(1) { prediction };
            _followOn = followOn;
        }
    }
}

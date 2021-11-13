using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    class NascentWordPredictionList
    {
        internal WordPrediction _core;
        internal List<WordPrediction> _compound;
        internal WordPrediction _followOn;
    }
}

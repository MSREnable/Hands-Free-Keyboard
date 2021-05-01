using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    class CaseWordVocabularySource : VocabularySource
    {
        private readonly WordVocabularySource _source;
        private readonly HeadWordItem _target;

        private readonly string[] _substitutes;

        internal CaseWordVocabularySource(ApplicationModel model, WordVocabularySource source, HeadWordItem target)
            : base(model)
        {
            _source = source;
            _target = target;

            _substitutes = new[] { "One", "tWo", "thRee", "fouR", "five", "SIX" };
        }

        internal override int Count => _substitutes.Length;

        internal override IEnumerable<ITile> CreateSuggestionList(int index)
        {
            var tile = new ReplacementItem(_target, _source, _substitutes[index]);
            var tiles = new[] { tile };
            return tiles;
        }

        internal override IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count)
        {
            var valueCount = Math.Min(limIndex - minIndex, count);
            var value = new int[valueCount];
            for (var i = 0; i < valueCount; i++)
            {
                value[i] = minIndex + i;
            }
            return value;
        }
    }
}

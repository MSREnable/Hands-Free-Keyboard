using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class SettingsVocabularySource : VocabularySource
    {
        public SettingsVocabularySource(ApplicationModel model)
            : base(model)
        {
        }

        internal override int Count => 1;

        internal override IEnumerable<ITile> CreateSuggestionList(int index)
        {
            Debug.Assert(index == 0);

            var value = new AdHocItem(Model, () => { }, TileType.Command, "Change setting");
            yield return value;
        }

        internal override IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count)
        {
            var limit = Math.Min(minIndex + count, limIndex);
            for (var i = minIndex; i < limit; i++)
            {
                yield return i;
            }
        }
    }
}
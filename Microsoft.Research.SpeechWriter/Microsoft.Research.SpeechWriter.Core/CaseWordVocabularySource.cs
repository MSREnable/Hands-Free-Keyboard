using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

            var map = WordCaseMap.Create(target.Content);

            var substitutes = new List<string>();

            if (map.LetterCount != 0)
            {
                var content = target.Content;

                if (map.UpperCount == 0 && map.LowerCount != 0)
                {
                    var position = map.Positions[0];
                    var title = content.Substring(0, position) + char.ToUpper(content[position]) + content.Substring(position + 1);
                    substitutes.Add(title);
                }

                if (map.UpperCount != 0)
                {
                    substitutes.Add(content.ToLower());
                }

                if (map.LowerCount != 0 && map.LetterCount != 1)
                {
                    substitutes.Add(content.ToUpper());
                }

                for (var i = 1; i < map.Positions.Length; i++)
                {
                    var position = map.Positions[i];
                    var ch = map.Uppers[i] ? char.ToLower(content[position]) : char.ToUpper(content[position]);
                    var cased = content.Substring(0, position) + ch + content.Substring(position + 1);
                    substitutes.Add(cased);
                }
            }

            _substitutes = substitutes.ToArray();
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

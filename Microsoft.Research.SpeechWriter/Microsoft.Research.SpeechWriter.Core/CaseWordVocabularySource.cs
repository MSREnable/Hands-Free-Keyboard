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

                var included = new HashSet<string> { content };

                var position0 = map.Positions[0];

                CheckedAdd(model.HeadItems[0].Culture.TextInfo.ToTitleCase(content));
                CheckedAdd(content.ToLower());
                CheckedAdd(content.ToUpper());
                CheckedAdd(content.Substring(0, position0) + char.ToUpper(content[position0]) + content.Substring(position0 + 1));

                for (var i = 0; i < map.Positions.Length; i++)
                {
                    var position = map.Positions[i];
                    var ch = map.Uppers[i] ? char.ToLower(content[position]) : char.ToUpper(content[position]);
                    var cased = content.Substring(0, position) + ch + content.Substring(position + 1);
                    CheckedAdd(cased);
                }

                void CheckedAdd(string version)
                {
                    if (included.Add(version))
                    {
                        substitutes.Add(version);
                    }
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

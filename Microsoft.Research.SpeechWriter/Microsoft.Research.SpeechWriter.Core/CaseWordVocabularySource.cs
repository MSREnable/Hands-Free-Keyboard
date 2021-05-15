using Microsoft.Research.SpeechWriter.Core.Data;
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

            var substitutes = new List<string>();

            var tile = TileData.FromTokenString(target.Content);
            var content = tile.Content;
            var isGlueAfter = tile.IsGlueAfter;
            var isGlueBefore = tile.IsGlueBefore;
            var included = new HashSet<string> { target.Content };

            var map = WordCaseMap.Create(content);

            if (map.LetterCount != 0)
            {
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

                void CheckedAdd(string newContent)
                {
                    var newTile = new TileData(content: newContent, isGlueAfter: isGlueAfter, isGlueBefore: isGlueBefore);
                    var version = newTile.ToTokenString();
                    if (included.Add(version))
                    {
                        substitutes.Add(version);
                    }
                }
            }

            for (var i = 0; i < 4; i++)
            {
                var spacedTile = new TileData(content: content, isGlueAfter: (i & 1) != 0, isGlueBefore: (i & 2) != 0);
                var spacedContent = spacedTile.ToTokenString();
                if (included.Add(spacedContent))
                {
                    substitutes.Add(spacedContent);
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

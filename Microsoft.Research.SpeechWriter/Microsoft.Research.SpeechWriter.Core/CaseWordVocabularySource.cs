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

            var lowerCount = 0;
            var upperCount = 0;
            var positions = new List<int>();
            var uppers = new List<bool>();

            var content = target.Content;
            for (var position = 0; position < content.Length; position++)
            {
                var ch = content[position];

                if (char.IsLetter(ch))
                {
                    var lower = char.ToLower(ch);
                    var upper = char.ToUpper(ch);
                    if (lower != upper)
                    {
                        if (ch == lower)
                        {
                            if (ch != upper)
                            {
                                lowerCount++;
                                positions.Add(position);
                                uppers.Add(false);
                            }
                        }
                        else if (ch == upper)
                        {
                            if (ch != lower)
                            {
                                upperCount++;
                                positions.Add(position);
                                uppers.Add(true);
                            }
                        }
                    }
                }
            }

            Debug.Assert(positions.Count == uppers.Count);

            var substitutes = new List<string>();

            if (positions.Count != 0)
            {
                if (upperCount == 0 && lowerCount != 0)
                {
                    var position = positions[0];
                    var title = content.Substring(0, position) + char.ToUpper(content[position]) + content.Substring(position + 1);
                    substitutes.Add(title);
                }

                if (upperCount != 0)
                {
                    substitutes.Add(content.ToLower());
                }

                if (lowerCount != 0 && lowerCount + upperCount != 1)
                {
                    substitutes.Add(content.ToUpper());
                }

                for (var i = 1; i < positions.Count; i++)
                {
                    var position = positions[i];
                    var ch = uppers[i] ? char.ToLower(content[position]) : char.ToUpper(content[position]);
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

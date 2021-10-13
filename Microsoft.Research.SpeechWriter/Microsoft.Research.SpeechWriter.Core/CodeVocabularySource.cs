using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class CodeVocabularySource : VocabularySource
    {
        private readonly WordVocabularySource _source;
        private string _code = string.Empty;


        internal CodeVocabularySource(WordVocabularySource source)
            : base(source.Model)
        {
            _source = source;
        }

        internal override int Count => _code.Length == 0 ? 5 : 6;

        private void SetCode(string code)
        {
            _code = code;
            this.SetSuggestionsView();
        }

        private IReadOnlyList<ITile> CreateCodes(params string[] labels)
        {
            var enumerable = new ITile[labels.Length];

            for (var index = 0; index < labels.Length; index++)
            {
                var code = _code + labels[index];
                var visualization = new TileVisualization(new ActionCommand(() => SetCode(code)), TileType.Normal, labels[index], TileColor.Text, TileColor.SuggestionPartBackground);
                enumerable[index] = new AdHocItem(Model, labels[index], visualization);
            }

            return enumerable;
        }

        internal override IReadOnlyList<ITile> CreateSuggestionList(int index)
        {
            IReadOnlyList<ITile> enumerable;

            switch (index)
            {
                case 0:
                    ITile item;
                    if (_code.Length == 0)
                    {
                        item = new AdHocItem(Model, () => { }, TileType.Command, "Enter code");
                    }
                    else
                    {
                        var prefix = _code.Substring(0, _code.Length - 1);
                        item = new AdHocItem(Model, () => SetCode(prefix), TileType.Command, prefix);
                    }
                    enumerable = new ITile[] { item };
                    break;
                case 1:
                    enumerable = CreateCodes("7", "8", "9");
                    break;
                case 2:
                    enumerable = CreateCodes("4", "5", "6");
                    break;
                case 3:
                    enumerable = CreateCodes("1", "2", "3");
                    break;
                case 4:
                    enumerable = CreateCodes("-", "0", ".");
                    break;
                default:
                    enumerable = new ITile[] { new SuggestedWordItem(Model.LastTile, _source, _code) };
                    break;
            }

            return enumerable;
        }

        internal override IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count)
        {
            var trueLim = Math.Min(limIndex, minIndex + count);
            for (var index = minIndex; index < trueLim; index++)
            {
                yield return index;
            }
        }
    }
}

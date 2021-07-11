using Microsoft.Research.SpeechWriter.Core.Data;
using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    class CodeVocabularySource : VocabularySource
    {
        private string code = string.Empty;

        internal CodeVocabularySource(ApplicationModel model)
            : base(model)
        {
        }

        internal override int Count => 5;

        private IEnumerable<ITile> CreateCodes(params string[] labels)
        {
            var enumerable = new ITile[labels.Length];

            for (var index = 0; index < labels.Length; index++)
            {
                enumerable[index] = new AdHocItem(Model, TileType.Normal, labels[index]);
            }

            return enumerable;
        }

        internal override IEnumerable<ITile> CreateSuggestionList(int index)
        {
            IEnumerable<ITile> enumerable;

            switch (index)
            {
                case 0:
                    var prefix = code.Length != 0 ? code.Substring(0, code.Length - 1) : "Enter code";
                    var item = new AdHocItem(Model, TileType.Command, prefix);
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
                    enumerable = new ITile[] { new AdHocItem(Model, TileType.Normal, code) };
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

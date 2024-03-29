﻿using Microsoft.Research.SpeechWriter.Core.Items;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class UnicodeVocabularySource : NonLinearIntegerRangeVocabularySource
    {
        private const int CodeMinimum = 0x21;
        private const int CodeLimit = 0x10000;
        private const int SurrogateCodePointsMinimum = 0xd800;
        private const int SurrogateCodePointsLimit = 0xe000;

        private readonly SpellingVocabularySource _parent;

        internal UnicodeVocabularySource(ApplicationModel model, SpellingVocabularySource parent)
            : base(model, CodeMinimum, CodeLimit - (SurrogateCodePointsLimit - SurrogateCodePointsMinimum))
        {
            _parent = parent;
        }

        internal override ITile CreateItem(int value)
        {
            var unicode = value < SurrogateCodePointsMinimum ?
                value : value - SurrogateCodePointsMinimum + SurrogateCodePointsLimit;
            var item = new SuggestedUnicodeItem(Model.LastTile, _parent, unicode);
            return item;
        }
    }
}

using Microsoft.Research.SpeechWriter.Core.Items;
using System.Windows.Input;

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

        private readonly OuterSpellingVocabularySource _parent;

        internal UnicodeVocabularySource(ApplicationModel model, OuterSpellingVocabularySource parent)
            : base(model, CodeMinimum, CodeLimit - (SurrogateCodePointsLimit - SurrogateCodePointsMinimum))
        {
            _parent = parent;
        }

        internal override ICommand CreateItem(int value)
        {
            var unicode = value < SurrogateCodePointsMinimum ? 
                value : value - SurrogateCodePointsMinimum + SurrogateCodePointsLimit;
            var item = new SuggestedUnicodeItem(_parent, unicode);
            return item;
        }
    }
}

using Microsoft.Research.SpeechWriter.Core.Data;
using System.Globalization;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Start item.
    /// </summary>
    public class HeadStartItem : Command<WordVocabularySource>
    {
        internal HeadStartItem(WordVocabularySource source)
            : base(null, source)
        {
        }

        /// <summary>
        /// The tile language.
        /// </summary>
        public override CultureInfo Culture => CultureInfo.CurrentUICulture;

        /// <summary>
        /// Is this item changed by conversion to uppercase or lowercase?
        /// </summary>
        public override bool IsCased => false;

        /// <summary>
        /// Does this item follow an item with IsCase true?
        /// </summary>
        public override bool IsCasedSuccessor => false;

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => "<<";

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, "\xAB");

        internal override void Execute(WordVocabularySource source)
        {
            source.TransferSuccessorsToRunOn(this);
        }
    }
}

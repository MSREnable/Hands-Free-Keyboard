using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Non-item for gaps.
    /// </summary>
    public class InterstitialNonItem : Command<WordVocabularySource>, ICommand
    {
        internal InterstitialNonItem(ApplicationModel model)
            : base(null, model)
        {
        }

        bool ICommand.CanExecute(object parameter) => false;

        internal override void Execute(WordVocabularySource source)
        {
        }


        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => "~";
    }
}

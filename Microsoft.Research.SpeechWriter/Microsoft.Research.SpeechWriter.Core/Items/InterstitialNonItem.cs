using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Non-item for gaps.
    /// </summary>
    public class InterstitialNonItem : Command<WordVocabularySource>, ICommand
    {
        internal InterstitialNonItem()
            : base(null, null)
        {
        }

        bool ICommand.CanExecute(object parameter) => false;

        internal override void Execute(WordVocabularySource source)
        {
        }

        /// <summary>
        /// ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => "~";
    }
}

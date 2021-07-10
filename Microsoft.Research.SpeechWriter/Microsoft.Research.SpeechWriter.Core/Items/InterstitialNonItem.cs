using Microsoft.Research.SpeechWriter.Core.Data;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Non-item for gaps.
    /// </summary>
    public class InterstitialNonItem : Command, ICommand
    {
        internal InterstitialNonItem(ApplicationModel model)
            : base(null, model)
        {
        }

        bool ICommand.CanExecute(object parameter) => false;

        internal override void Execute()
        {
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, TileVisualizationType.Hidden);

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => "~";
    }
}

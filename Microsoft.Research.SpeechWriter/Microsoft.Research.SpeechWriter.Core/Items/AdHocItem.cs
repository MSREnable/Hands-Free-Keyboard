using Microsoft.Research.SpeechWriter.Core.Data;
using System;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    internal class AdHocItem : Command
    {
        internal AdHocItem(ApplicationModel model, string content, TileVisualization visualization)
            :base(null, model)
        {
            Content = content;
            Visualization = visualization;
        }

        internal AdHocItem(ApplicationModel model, TileType type, string content)
            : this(model, content, new TileVisualization((ICommand)null, type, content, TileColor.Text, TileColor.SuggestionBackground))
        {

        }

        public override string Content { get; }

        public override TileVisualization Visualization { get; }

        internal override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}

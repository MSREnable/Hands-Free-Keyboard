using Microsoft.Research.SpeechWriter.Core.Data;
using System;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    internal class AdHocItem : Command
    {
        internal AdHocItem(ApplicationModel model, string content, TileVisualization visualization)
            : base(null, model)
        {
            Content = content;
            Visualization = visualization;
        }

        internal AdHocItem(ApplicationModel model, Action action, TileType type, string content)
            : this(model, content, new TileVisualization(new ActionCommand(action), type, content, TileColor.Text, TileColor.SuggestionBackground))
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

using Microsoft.Research.SpeechWriter.Core.Data;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Whole word item.
    /// </summary>
    public sealed class CommandItem : Command<WordVocabularySource>
    {
        private readonly TileCommand _command;

        internal CommandItem(ITile predecessor, WordVocabularySource source, TileCommand command)
            : base(predecessor, source)
        {
            _command = command;
        }

        /// <summary>
        /// The command.
        /// </summary>
        public TileCommand Command => _command;

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => _command.ToString();

        /// <summary>
        /// The formatted content of the tile.
        /// </summary>
        public override string FormattedContent => _command.ToString();

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, _command.ToString());

        internal override void Execute()
        {
            Source.ExecuteCommand(_command);
        }

        internal override void TraceContent(XmlWriter writer)
        {
            writer.WriteAttributeString(nameof(_command), _command.ToString());
        }
    }
}
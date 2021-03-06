﻿using Microsoft.Research.SpeechWriter.Core.Data;
using System;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// Start item.
    /// </summary>
    public class TailStopItem : Command<WordVocabularySource>, ISuggestionItem
    {
        internal TailStopItem(ITile predecessor, WordVocabularySource source)
            : base(predecessor, source)
        {
        }

        /// <summary>
        /// Visualization description.
        /// </summary>
        public override TileVisualization Visualization => new TileVisualization(this, "\xBB");

        internal override void Execute()
        {
            Source.Commit(this);
        }

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The basic content of the tile.
        /// </summary>
        public override string Content => ">>";
    }
}

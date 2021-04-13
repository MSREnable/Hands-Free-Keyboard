﻿using System.Globalization;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Interface implemented by all SpeechWriter tiles.
    /// </summary>
    public interface ITile : ICommand
    {
        /// <summary>
        /// Tile that preceeds this.
        /// </summary>
        ITile Predecessor { get; }

        /// <summary>
        /// The language information.
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// Is this item changed by conversion to uppercase or lowercase?
        /// </summary>
        bool IsCased { get; }

        /// <summary>
        /// Does this item follow an item with IsCase true?
        /// </summary>
        bool IsCasedSuccessor { get; }
    }
}

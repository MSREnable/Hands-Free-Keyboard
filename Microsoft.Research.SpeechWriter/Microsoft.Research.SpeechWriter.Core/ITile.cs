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
    }
}

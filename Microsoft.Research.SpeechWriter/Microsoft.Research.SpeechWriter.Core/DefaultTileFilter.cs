namespace Microsoft.Research.SpeechWriter.Core
{
    internal class DefaultTileFilter : ITokenTileFilter
    {
        private static ITokenTileFilter _instance = new DefaultTileFilter();

        private DefaultTileFilter()
        {
        }

        internal static ITokenTileFilter Instance => _instance;

        bool ITileFilter.IsIndexVisible(int index) => true;

        bool ITokenTileFilter.IsTokenVisible(int token) => true;
    }
}
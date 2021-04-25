using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class WordTileFiler : TileFilter
    {
        private readonly StringTokens _tokens;
        private readonly CompareInfo _compare;
        private readonly List<string> _set = new List<string>();
        private readonly Dictionary<int, bool> _tokenToAcceptance = new Dictionary<int, bool>();

        public WordTileFiler(StringTokens tokens, CultureInfo culture)
        {
            _tokens = tokens;
            _compare = culture.CompareInfo;
        }

        internal override bool IsTokenVisible(int token)
        {
            if (!_tokenToAcceptance.TryGetValue(token, out var value))
            {
                var word = _tokens.GetString(token);

                // If there is content before a null character...
                var nullPosition = word.IndexOf('\0');
                if (0 < nullPosition)
                {
                    // ...ignore the content beyond the null.
                    word = word.Substring(0, nullPosition);
                }

                value = true;
                using (var enumerator = _set.GetEnumerator())
                {
                    while (value && enumerator.MoveNext())
                    {
                        if (_compare.Compare(word, enumerator.Current, CompareOptions.IgnoreCase) == 0)
                        {
                            value = false;
                        }
                    }
                }

                if (value)
                {
                    _set.Add(word);
                }

                _tokenToAcceptance.Add(token, value);
            }

            return value;
        }
    }
}
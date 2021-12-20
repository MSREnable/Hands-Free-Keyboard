using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// Mapping between strings and integer tokens.
    /// </summary>
    /// <remarks>
    /// There is a tacit assumption that words with higher token values are to be ranked higher than those with a lower token value.
    /// </remarks>
    public class StringTokens
    {
        private int _tokenLimit;

        private readonly List<string> _tokenToString = new List<string>();

        private readonly Dictionary<string, int> _stringToToken = new Dictionary<string, int>();

        /// <summary>
        /// String representing the stop token.
        /// </summary>
        public const string StopString = "\0";

        /// <summary>
        /// Token representing the stop string.
        /// </summary>
        public const int StopToken = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StringTokens()
        {
            _tokenToString.Add(StopString);
            _stringToToken.Add(StopString, StopToken);
            _tokenLimit = 1;
        }

        /// <summary>
        /// Initializing constructor.
        /// </summary>
        /// <param name="enumerable">Strings in reverse rank order.</param>
        public StringTokens(IEnumerable<string> enumerable)
            : this()
        {
            var descendingRankStrings = new List<string>(enumerable);

            for (var index = descendingRankStrings.Count - 1; 0 <= index; index--)
            {
                var str = string.Intern(descendingRankStrings[index]);
                var token = _tokenLimit;
                _tokenLimit++;

                _stringToToken.Add(str, token);
                _tokenToString.Add(str);
            }
        }

        /// <summary>
        /// The minimum value for a valid token.
        /// </summary>
        public int TokenStart => 1;

        /// <summary>
        /// The current limit value of the contiguos token values.
        /// </summary>
        public int TokenLimit => _tokenLimit;

        /// <summary>
        /// Get string from token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The string.</returns>
        public string this[int token] => GetString(token);

        /// <summary>
        /// Get token from string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The token.</returns>
        public int this[string str] => GetToken(str);

        /// <summary>
        /// Create a default set of English word tokens.
        /// </summary>
        /// <returns>A StringTokens object.</returns>
        public static StringTokens Create(IEnumerable<string> seed)
        {
            var value = new StringTokens(seed);

            return value;
        }

        /// <summary>
        /// Get string from token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The string.</returns>
        public string GetString(int token)
        {
            Debug.Assert(0 <= token);
            Debug.Assert(token < _tokenLimit);

            var str = _tokenToString[token];

            return str;
        }

        /// <summary>
        /// Get token from string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The token.</returns>
        public int GetToken(string str)
        {
            Debug.Assert(!string.IsNullOrEmpty(str));

            if (!_stringToToken.TryGetValue(str, out var token))
            {
                token = _tokenLimit;
                _tokenLimit++;

                _stringToToken.Add(str, token);
                _tokenToString.Add(str);

                Debug.Assert(_stringToToken.Count == _tokenToString.Count);
            }

            return token;
        }

        /// <summary>
        /// Get token from string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The token.</returns>
        public bool TryGetToken(string str, out int token)
        {
            Debug.Assert(!string.IsNullOrEmpty(str));

            var value = _stringToToken.TryGetValue(str, out token);
            return value;
        }

        internal bool IsNewWord(string str)
        {
            Debug.Assert(!string.IsNullOrEmpty(str));

            var result = !_stringToToken.ContainsKey(str);

            if (result)
            {
                _ = GetToken(str);
            }

            return result;
        }
    }
}

using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class WordRepeatTokenFiler : RepeatTokenFilter
    {
        private readonly StringTokens _tokens;
        private readonly CompareInfo _compare;
        private readonly List<string> _set = new List<string>();

        public WordRepeatTokenFiler(StringTokens tokens, CultureInfo culture)
        {
            _tokens = tokens;
            _compare = culture.CompareInfo;
        }

        internal override bool Accept(int token)
        {
            var word = _tokens.GetString(token);

            var value = true;
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

            return value;
        }
    }
}
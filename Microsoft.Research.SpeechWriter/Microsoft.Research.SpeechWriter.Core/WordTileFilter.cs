using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class WordTileFilter : ITokenTileFilter
    {
        private readonly WordVocabularySource _source;
        private readonly StringTokens _tokens;
        private readonly CompareInfo _compare;
        private readonly List<string> _set = new List<string>();
        private readonly Dictionary<int, bool> _tokenToAcceptance = new Dictionary<int, bool>();

        public WordTileFilter(WordVocabularySource source, StringTokens tokens, CultureInfo culture)
        {
            _source = source;
            _tokens = tokens;
            _compare = culture.CompareInfo;
        }

        public bool IsIndexVisible(int index)
        {
            var token = _source.GetIndexToken(index);
            var value = IsTokenVisible(token);
            return value;
        }

        public bool IsTokenVisible(int token)
        {
            if (!_tokenToAcceptance.TryGetValue(token, out var value))
            {
                var word = _tokens.GetString(token);

                // If there is content before a null character...
                var nullPosition = word.IndexOf('\0');
                switch (nullPosition)
                {
                    case 0:
                        if (word == StringTokens.StopString)
                        {
                            value = true;
                        }
                        else
                        {
                            var command = (TileCommand)Enum.Parse(typeof(TileCommand), word.Substring(1));
                            value = _source.IsCommandVisible(command);
                        }
                        break;

                    default:
                        // ...ignore the content beyond the null.
                        word = word.Substring(0, nullPosition);
                        goto case -1;

                    case -1:
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
                        break;
                }
            }

            return value;
        }

        internal void Reset()
        {
            _set.Clear();
            _tokenToAcceptance.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal class TokenPredictorDatabase
    {
        private readonly Dictionary<int, TokenPredictorInfo> _database = new Dictionary<int, TokenPredictorInfo>();

        public IEnumerable<int> Keys => _database.Keys;

        private List<TokenPredictorInfo> _sortedDatabase;

        private bool _isSortedDatabaseValid;

        internal TokenPredictorInfo GetValue(int token)
        {
            if (!_database.TryGetValue(token, out var info))
            {
                info = new TokenPredictorInfo(token);
                _database.Add(token, info);

                if (_sortedDatabase != null)
                {
                    _sortedDatabase.Add(info);

                    _isSortedDatabaseValid = false;
                }
                Debug.Assert(!_isSortedDatabaseValid);
            }

            return info;
        }

        internal bool TryGetValue(int token, out TokenPredictorInfo info)
        {
            var value = _database.TryGetValue(token, out info);
            return value;
        }

        private void ClearSortedDatabase()
        {
            _sortedDatabase = null;
            _isSortedDatabaseValid = false;
        }

        internal void Clear()
        {
            _database.Clear();
            ClearSortedDatabase();
        }

        public Dictionary<int, TokenPredictorInfo>.Enumerator GetEnumerator()
        {
            return _database.GetEnumerator();
        }

        internal int Count => _database.Count;

        internal IEnumerable<TokenPredictorInfo> SortedEnumerable
        {
            get
            {
                ValidateSortedDatabase();

                return _sortedDatabase;
            }
        }

        internal void Remove(int token)
        {
            _database.Remove(token);

            ClearSortedDatabase();
        }

        private void ValidateSortedDatabase()
        {
            if (!_isSortedDatabaseValid)
            {
                if (_sortedDatabase == null)
                {
                    _sortedDatabase = new List<TokenPredictorInfo>();

                    _sortedDatabase.AddRange(_database.Values);
                }

                Debug.Assert(_database.Count == _sortedDatabase.Count);

                _sortedDatabase.Sort(SortedDictionaryComparison);

                _isSortedDatabaseValid = true;
            }
        }

        private int SortedDictionaryComparison(TokenPredictorInfo x,
            TokenPredictorInfo y)
        {
            var value = y.Count.CompareTo(x.Count);
            if (value == 0)
            {
                value = y.Token.CompareTo(x.Token);
            }
            return value;
        }

        internal IReadOnlyList<int> GetTopRanked()
        {
            ValidateSortedDatabase();

            var tokens = new List<int>();
            if (_sortedDatabase.Count != 0)
            {
                var count = _sortedDatabase[0].Count;
                tokens.Add(_sortedDatabase[0].Token);
                for (var i = 1; i < _sortedDatabase.Count && _sortedDatabase[i].Count == count; i++)
                {
                    tokens.Add(_sortedDatabase[i].Token);
                }
            }

            return tokens;
        }

        internal IReadOnlyList<int> GetTopRanked(IReadOnlyList<int> mask)
        {
            IReadOnlyList<int> result;

            ValidateSortedDatabase();

            var tokens = new List<int>();
            using (var enumerator = _sortedDatabase.GetEnumerator())
            {
                var read = enumerator.MoveNext();
                while (read && !mask.Contains(enumerator.Current.Token))
                {
                    read = enumerator.MoveNext();
                }

                if (read)
                {
                    var count = enumerator.Current.Count;
                    tokens.Add(enumerator.Current.Token);

                    while (enumerator.MoveNext() && enumerator.Current.Count == count)
                    {
                        if (mask.Contains(enumerator.Current.Token))
                        {
                            tokens.Add(enumerator.Current.Token);
                        }
                    }
                }
            }

            Debug.Assert(tokens.Count <= mask.Count);

            if (tokens.Count != 0)
            {
                result = tokens;
            }
            else
            {
                result = mask;
            }

            return result;
        }

        internal TokenPredictorDatabase GetChild(int[] context, int index, int length)
        {
            Debug.Assert(0 <= length);

            TokenPredictorDatabase result;

            if (length == 0)
            {
                result = this;
            }
            else if (_database.TryGetValue(context[index], out var info))
            {
                if (length == 1)
                {
                    result = info.TryGetChildren();
                }
                else
                {
                    result = info.GetChild(context, index + 1, length - 1);
                }
            }
            else
            {
                result = null;
            }

            return result;
        }

        internal void IncrementCount(TokenPredictorInfo child, int increment)
        {
            Debug.Assert(ReferenceEquals(child, _database[child.Token]));
            child.IncrementCount(increment);
            _isSortedDatabaseValid = false;
        }

        internal int DecrementCount(TokenPredictorInfo child, int increment)
        {
            Debug.Assert(ReferenceEquals(child, _database[child.Token]));
            var value = child.DecrementCount(increment);
            _isSortedDatabaseValid = false;

            return value;
        }

        internal void WriteXml(XmlWriter writer, Func<int, string> stringize)
        {
            foreach (var info in SortedEnumerable)
            {
                writer.WriteStartElement("Entry");
                var s = stringize(info.Token);
                s = s.Replace("\0", "\\0");
                writer.WriteAttributeString("Word", s);
                writer.WriteAttributeString("Count", info.Count.ToString());

                var children = info.TryGetChildren();
                if (children != null)
                {
                    children.WriteXml(writer, stringize);
                }

                writer.WriteEndElement();
            }
        }
    }
}

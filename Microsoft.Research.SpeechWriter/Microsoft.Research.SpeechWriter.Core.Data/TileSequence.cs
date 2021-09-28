using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    /// <summary>
    /// Container class for a sequence of <code>TileData</code> objects.
    /// </summary>
    public class TileSequence : IReadOnlyList<TileData>
    {
        /// <summary>
        /// A simple space that may be assumed before and after a sequence and also may be implied within a sequence.
        /// </summary>
        private static readonly TileData SingleSimpleSpace = TileData.Create(" ", isPrefix: true, isSuffix: true);

        /// <summary>
        /// Container for the <code>TileData</code> objects.
        /// </summary>
        private readonly List<TileData> _sequence;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sequence"></param>
        private TileSequence(List<TileData> sequence)
        {
            _sequence = sequence;
        }

        public int Count => _sequence.Count;

        public TileData this[int index] => _sequence[index];

        /// <summary>
        /// Constructor from an array of existing <code>TileData</code> objects.
        /// </summary>
        /// <param name="tiles">The tiles.</param>
        /// <returns>A constructed <code>TileSequence</code>.</returns>
        public static TileSequence FromData(params TileData[] tiles)
        {
            var list = new List<TileData>(tiles);
            var sequence = new TileSequence(list);
            return sequence;
        }

        /// <summary>
        /// Constructor from an array of existing <code>TileData</code> objects.
        /// </summary>
        /// <param name="tiles">The tiles.</param>
        /// <returns>A constructed <code>TileSequence</code>.</returns>
        public static TileSequence FromData(IEnumerable<TileData> tiles)
        {
            var list = new List<TileData>(tiles);
            var sequence = new TileSequence(list);
            return sequence;
        }

        /// <summary>
        /// Convert a raw string to the simple encoded format.
        /// </summary>
        /// <param name="raw">The raw string.</param>
        /// <returns>A simple encoded string.</returns>
        public static string RawToDefaultSimpleEncoded(string raw)
        {
            var encoded = XmlHelper.WriteXmlFragment(writer =>
            {
                writer.WriteString(raw);
            });
            return encoded;
        }

        ///// <summary>
        ///// Convert a raw string to the simple encoded format.
        ///// </summary>
        ///// <param name="encoded">The raw string.</param>
        ///// <returns>A simple encoded string.</returns>
        //public static string DefaultSimpleEncodedToRaw(string encoded) => encoded.ReadXmlFragment<string>(reader =>
        //{
        //    Debug.Assert(reader.NodeType == XmlNodeType.None);
        //    reader.ValidatedRead();
        //    Debug.Assert(reader.NodeType == XmlNodeType.Text);
        //    var raw = reader.Value;

        //    return raw;
        //});

        /// <summary>
        /// Convert to raw string.
        /// </summary>
        /// <returns>The undecorated text.</returns>
        public string ToRaw()
        {
            var output = new StringWriter();

            var isPreviousAttached = true;
            foreach (var tile in _sequence)
            {
                if (!isPreviousAttached && !tile.IsSuffix)
                {
                    output.Write(SingleSimpleSpace.Content);
                }
                output.Write(tile.Content);
                isPreviousAttached = tile.IsPrefix;
            }

            var value = output.ToString();
            return value;
        }

        /// <summary>
        /// Convert to the simple encoded format.
        /// </summary>
        /// <returns>The simple encoded version of this tile sequence.</returns>
        public string ToSimpleEncoded()
        {
            var output = new StringWriter();

            var value = XmlHelper.WriteXmlFragment(writer =>
            {
                var isPreviousAttached = true;
                foreach (var tile in _sequence)
                {
                    if (!isPreviousAttached && !tile.IsSuffix)
                    {
                        SingleSimpleSpace.ToXmlWriter(writer, false);
                    }
                    tile.ToXmlWriter(writer, false);
                    isPreviousAttached = tile.IsPrefix;
                }
            });

            return value;
        }

        public static TileSequence FromRaw(string raw)
        {
            var sequence = FromSimpleEncoded(raw);
            return sequence;
        }

        /// <summary>
        /// Convert to the tile encoded format.
        /// </summary>
        /// <returns>The tile encoded version of this tile sequence.</returns>
        public string ToTileEncoded()
        {
            var value = XmlHelper.WriteXmlFragment(writer =>
            {
                foreach (var tile in _sequence)
                {
                    tile.ToXmlWriter(writer, true);
                }
            });

            return value;
        }

        public string ToHybridEncoded()
        {
            // Try the simple approach...
            var value = ToSimpleEncoded();

            // ...but if that fails...
            var decode = FromSimpleEncoded(value);
            if (!Equals(decode))
            {
                // ...do it the full-fat way,
                value = ToTileEncoded();
            }

            return value;
        }

        public static TileSequence FromSimpleEncoded(string text)
        {
            var list = new List<TileData>();

            // The length of text to be processed.
            var length = text.Length;

            // The current processing position.
            var position = 0;

            // Count the amount of leading white space.
            while (position < length && text[position] == ' ')
            {
                position++;
            }
            if (position != 0)
            {
                // There is leading space, so emit explicit space.
                var tile = TileData.Create(new string(' ', position), isPrefix: true, isSuffix: true);
                list.Add(tile);
            }

            var previousTile = SingleSimpleSpace;
            while (position < length)
            {
                // Calculate start and limit of the next group.
                var groupStart = position;
                while (position < length && text[position] != ' ')
                {
                    position++;
                }
                var groupLimit = position;

                // Calculate the part of the group that is word.
                var wordStart = groupStart;
                while (wordStart < groupLimit && !char.IsLetterOrDigit(text[wordStart]))
                {
                    wordStart++;
                }

                // Calculate the part of the group that is, or is not, word.
                var wordLimit = groupLimit;
                if (wordStart < groupLimit)
                {
                    while (!char.IsLetterOrDigit(text[wordLimit - 1]))
                    {
                        wordLimit--;
                    }
                }

                // Skip over whitespace at end.
                while (position < length && text[position] == ' ')
                {
                    position++;
                }

                if (wordStart < wordLimit)
                {
                    // Emit leading symbolds, word and trailing symbols.
                    for (var i = groupStart; i < wordStart; i++)
                    {
                        var ch = text[i];
                        var tile = TileData.Create(TileType.Prefix, ch.ToString());
                        list.Add(tile);
                    }

                    if (wordStart < wordLimit)
                    {
                        var word = text.Substring(wordStart, wordLimit - wordStart);
                        var tile = TileData.Create(word);
                        list.Add(tile);

                        Debug.Assert(tile.IsSimpleWord);
                    }

                    for (var i = wordLimit; i < groupLimit; i++)
                    {
                        var ch = text[i];
                        var tile = TileData.Create(TileType.Suffix, ch.ToString());
                        list.Add(tile);
                    }
                }
                else
                {
                    Debug.Assert(previousTile.IsSpaces);
                    if (groupLimit < position || groupLimit == length)
                    {
                        // Emit as one string.
                        var group = text.Substring(groupStart, groupLimit - groupStart);
                        var tile = TileData.Create(group, isSuffix: false);
                        list.Add(tile);
                    }
                }

                // Add explicit whitespace if needed.
                if (groupLimit + 1 < position || (groupLimit < position && position == length))
                {
                    var tile = TileData.Create(new string(' ', position - groupLimit), isPrefix: true, isSuffix: true);
                    list.Add(tile);

                    Debug.Assert(tile.IsSpaces);
                }
            }

            var sequence = new TileSequence(list);
            return sequence;
        }

        internal static TileSequence FromEncoded(XmlReader reader, XmlNodeType endNode)
        {
            TileSequence sequence;

            if (reader.NodeType != endNode)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                        var text = reader.Value;
                        reader.Read();

                        sequence = FromSimpleEncoded(text);
                        break;

                    case XmlNodeType.Element:
                        var tiles = new List<TileData>();
                        while (reader.NodeType == XmlNodeType.Element)
                        {
                            var tile = TileData.FromXmlReader(reader);
                            tiles.Add(tile);
                        }
                        sequence = FromData(tiles.ToArray());
                        break;

                    default:
                        throw new InvalidDataException();
                }
            }
            else
            {
                sequence = FromData();
            }

            XmlHelper.ValidateNodeType(reader, endNode);

            return sequence;
        }

        public static TileSequence FromEncoded(string encoded) => encoded.ReadXmlFragment<TileSequence>(reader =>
          {
              reader.Read();

              var value = FromEncoded(reader, XmlNodeType.None);

              return value;
          });

        public bool Equals(TileSequence sequence)
        {
            var count = _sequence.Count;
            var value = count == sequence._sequence.Count;

            for (var i = 0; value && i < count; i++)
            {
                value = _sequence[i].Equals(sequence._sequence[i]);
            }

            return value;
        }

        public override bool Equals(object obj)
        {
            var value = obj is TileSequence sequence && Equals(sequence);
            return value;
        }

        public override int GetHashCode()
        {
            return _sequence.GetHashCode();
        }

        public IEnumerator<TileData> GetEnumerator()
        {
            return _sequence.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_sequence).GetEnumerator();
        }
    }
}

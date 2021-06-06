using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

[assembly: InternalsVisibleTo("Microsoft.Research.SpeechWriter.Core.Data.Test")]

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    public class TileData
    {
        private static readonly string[] _elementNames = new[] { "T", "B", "A", "J" };

        private TileData(string content)
            : this(content, isGlueBefore: false, isGlueAfter: false)
        {
        }

        private TileData(string content,
            bool isGlueBefore = false,
            bool isGlueAfter = false,
            IReadOnlyDictionary<string, string> attributes = null)
        {
            if (content.Contains("\0"))
            {
                throw new ArgumentException("content contains null character");
            }

            Content = content;
            IsGlueBefore = isGlueBefore;
            IsGlueAfter = isGlueAfter;
            Attributes = attributes != null && attributes.Count != 0 ? attributes : null;

            Debug.Assert(!IsSpaces || (IsGlueBefore & IsGlueAfter), "Spaces must always be glued both sides");
        }

        public static TileData Create(string content)
        {
            var value = new TileData(content);
            return value;
        }

        public static TileData Create(string content,
            bool isGlueBefore = false,
            bool isGlueAfter = false,
            IReadOnlyDictionary<string, string> attributes = null)
        {
            var value = new TileData(content: content,
                isGlueBefore: isGlueBefore,
                isGlueAfter: isGlueAfter,
                attributes: attributes);
            return value;
        }

        public string ToTokenString()
        {
            string value;

            if (Attributes != null)
            {
                var list = new SortedSet<string>();

                var nameIndex = (IsGlueAfter ? 1 : 0) + (IsGlueBefore ? 2 : 0);
                if (nameIndex != 0)
                {
                    var localName = _elementNames[nameIndex];
                    list.Add(localName);
                }

                foreach (var pair in Attributes)
                {
                    var attribute = $"{pair.Key}={pair.Value}";
                    list.Add(attribute);
                }
                value = Content + '\0' + string.Join("\0", list);
            }
            else if (IsGlueBefore || IsGlueAfter)
            {
                var nameIndex = (IsGlueAfter ? 1 : 0) + (IsGlueBefore ? 2 : 0);
                var localName = _elementNames[nameIndex];
                value = $"{Content}\0{localName}";
            }
            else
            {
                value = Content;
            }

            return value;
        }

        public static TileData FromTokenString(string token)
        {
            TileData value;

            var splits = token.Split('\0');
            if (splits.Length == 1)
            {
                value = TileData.Create(token);
            }
            else
            {
                var content = splits[0];
                var nameIndex = 0;

                Dictionary<string, string> attributes = null;

                for (var i = 1; i < splits.Length; i++)
                {
                    var attributeKeyValue = splits[i];

                    if (attributeKeyValue.Length == 1)
                    {
                        nameIndex = Array.IndexOf(_elementNames, attributeKeyValue);
                    }
                    else
                    {
                        if (attributes == null)
                        {
                            attributes = new Dictionary<string, string>();
                        }

                        var keyValue = attributeKeyValue.Split(new[] { '=' }, 2);
                        attributes.Add(keyValue[0], keyValue[1]);
                    }
                }

                value = TileData.Create(content: content,
                    isGlueBefore: (nameIndex & 2) != 0,
                    isGlueAfter: (nameIndex & 1) != 0,
                    attributes: attributes);
            }

            return value;
        }

        public string Content { get; }

        public IReadOnlyDictionary<string, string> Attributes { get; }

        public string this[string key]
        {
            get
            {
                string value;

                if (Attributes != null)
                {
                    Attributes.TryGetValue(key, out value);
                }
                else
                {
                    value = null;
                }

                return value;
            }
        }

        private bool IsNoGlue => !IsGlueBefore && !IsGlueAfter;

        /// <summary>
        /// Attach without space to the previous item.
        /// </summary>
        [XmlIgnore]
        public bool IsGlueAfter { get; }

        /// <summary>
        /// Attach without space to the next item.
        /// </summary>
        [XmlIgnore]
        public bool IsGlueBefore { get; }

        [XmlIgnore]
        internal bool IsSimpleWord => IsNoGlue && 0 < Content.Length && char.IsLetterOrDigit(Content[0]) && char.IsLetterOrDigit(Content[Content.Length - 1]) &&
            !Content.Contains(" ");

        [XmlIgnore]
        internal bool IsSpaces => Content.Replace(" ", string.Empty) == string.Empty;

        internal void ToXmlWriter(XmlWriter writer, bool isElemental)
        {
            if (isElemental)
            {
                var nameIndex = (IsGlueAfter ? 1 : 0) + (IsGlueBefore ? 2 : 0);
                var localName = _elementNames[nameIndex];
                writer.WriteStartElement(localName);

                if (Attributes != null)
                {
                    var dictionary = new SortedDictionary<string, string>();

                    foreach (var pair in Attributes)
                    {
                        dictionary.Add(pair.Key, pair.Value);
                    }

                    foreach (var pair in dictionary)
                    {
                        writer.WriteAttributeString(pair.Key, pair.Value);
                    }
                }
            }
            writer.WriteString(Content);
            if (isElemental)
            {
                writer.WriteEndElement();
            }
        }

        internal static TileData FromXmlReader(XmlReader reader)
        {
            reader.ValidateNodeType(XmlNodeType.Element);
            var nameIndex = Array.IndexOf(_elementNames, reader.Name);
            XmlHelper.ValidateData(0 <= nameIndex);

            Dictionary<string, string> attributes;
            if (reader.MoveToFirstAttribute())
            {
                attributes = new Dictionary<string, string>();

                do
                {
                    attributes.Add(reader.Name, reader.Value);
                }
                while (reader.MoveToNextAttribute());
            }
            else
            {
                attributes = null;
            }
            reader.Read();

            reader.ValidateNodeType(XmlNodeType.Text);
            var text = reader.Value;
            var tile = TileData.Create(content: text,
                isGlueBefore: (nameIndex & 2) != 0,
                isGlueAfter: (nameIndex & 1) != 0,
                attributes: attributes);

            reader.Read();
            reader.ReadNodeType(XmlNodeType.EndElement);

            return tile;
        }

        public bool Equals(TileData element)
        {
            var value = Content == element.Content &&
                IsGlueBefore == element.IsGlueBefore &&
                IsGlueAfter == element.IsGlueAfter;

            if (value)
            {
                if (Attributes != null)
                {
                    value = element.Attributes != null &&
                        Attributes.Count == element.Attributes.Count;

                    if (value)
                    {
                        using (var enumerable = Attributes.GetEnumerator())
                        {
                            while (value && enumerable.MoveNext())
                            {
                                value = enumerable.Current.Value ==
                                    element[enumerable.Current.Key];
                            }
                        }
                    }
                }
                else
                {
                    value = element.Attributes == null;
                }
            }

            return value;
        }

        public override bool Equals(object obj)
        {
            var element = obj as TileData;
            return element != null && Equals(element);
        }

        public override int GetHashCode()
        {
            return Content.GetHashCode() ^ IsGlueBefore.GetHashCode() ^ IsGlueAfter.GetHashCode();
        }

        public override string ToString()
        {
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };
            var output = new StringWriter();
            using (var writer = XmlWriter.Create(output, settings))
            {
                ToXmlWriter(writer, true);
            }
            var value = output.ToString();
            return value;
        }
    }
}

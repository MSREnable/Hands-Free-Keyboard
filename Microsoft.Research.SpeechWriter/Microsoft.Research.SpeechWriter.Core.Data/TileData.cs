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
        private TileData(TileType type,
            string content,
            IReadOnlyDictionary<string, string> attributes = null)
        {

            if (content.Contains("\0"))
            {
                throw new ArgumentException("content contains null character");
            }

            Type = type;
            Content = content;
            Attributes = attributes != null && attributes.Count != 0 ? attributes : null;

            Debug.Assert(!IsSpaces || (IsSuffix & IsPrefix), "Spaces must always be glued both sides");
        }

        public static TileData Create(string content)
        {
            var value = new TileData(TileType.Normal, content);
            return value;
        }

        public static TileData Create(string content,
            bool isPrefix = false,
            bool isSuffix = false,
            IReadOnlyDictionary<string, string> attributes = null)
        {
            var type = TileTypeHelper.TypeFromGlue(isPrefix: isPrefix, isSuffix: isSuffix);
            var value = new TileData(type: type,
                content: content,
                attributes: attributes);
            return value;
        }

        public static TileData Create(TileType type,
            string content,
            IReadOnlyDictionary<string, string> attributes = null)
        {
            var value = new TileData(type: type,
                content: content,
                attributes: attributes);
            return value;
        }

        public string ToTokenString()
        {
            var value = Content;

            if (Type != TileType.Normal)
            {
                value += '\0' + Type.ToElementName();
            }

            if (Attributes != null)
            {
                var list = new SortedSet<string>();

                foreach (var pair in Attributes)
                {
                    var attribute = $"{pair.Key}={pair.Value}";
                    list.Add(attribute);
                }

                value += '\0' + string.Join("\0", list);
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
                var type = TileType.Normal;

                Dictionary<string, string> attributes = null;

                for (var i = 1; i < splits.Length; i++)
                {
                    var attributeKeyValue = splits[i];

                    if (attributeKeyValue.Length == 1)
                    {
                        type = TileTypeHelper.FromElementName(attributeKeyValue);
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
                    type: type,
                    attributes: attributes);
            }

            return value;
        }

        public TileType Type { get; }

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

        private bool IsNoGlue => !IsSuffix && !IsPrefix;

        /// <summary>
        /// Attach without space to the previous item.
        /// </summary>
        [XmlIgnore]
        public bool IsPrefix => Type.IsPrefix();

        /// <summary>
        /// Attach without space to the next item.
        /// </summary>
        [XmlIgnore]
        public bool IsSuffix => Type.IsSuffix();

        [XmlIgnore]
        internal bool IsSimpleWord => IsNoGlue && 0 < Content.Length && char.IsLetterOrDigit(Content[0]) && char.IsLetterOrDigit(Content[Content.Length - 1]) &&
            !Content.Contains(" ");

        [XmlIgnore]
        internal bool IsSpaces => Content.Replace(" ", string.Empty) == string.Empty;

        internal void ToXmlWriter(XmlWriter writer, bool isElemental)
        {
            if (isElemental)
            {
                var localName = Type.ToElementName();
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
            var type = TileTypeHelper.FromElementName(reader.Name);

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
            var content = reader.Value;
            var tile = TileData.Create(type: type,
                content: content,
                attributes: attributes);

            reader.Read();
            reader.ReadNodeType(XmlNodeType.EndElement);

            return tile;
        }

        public bool Equals(TileData element)
        {
            var value = Content == element.Content &&
                IsSuffix == element.IsSuffix &&
                IsPrefix == element.IsPrefix;

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
            return Content.GetHashCode() ^ IsSuffix.GetHashCode() ^ IsPrefix.GetHashCode();
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

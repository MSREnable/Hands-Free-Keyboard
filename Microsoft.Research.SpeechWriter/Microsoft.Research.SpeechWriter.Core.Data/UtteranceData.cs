using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    public class UtteranceData
    {
        public UtteranceData(TileSequence sequence,
            DateTimeOffset? started = null,
            TimeSpan? duration = null,
            int? keyCount = null)
        {
            Sequence = sequence;
            Started = started;
            Duration = duration;
            KeyCount = keyCount;
        }

        public TileSequence Sequence { get; }

        public DateTimeOffset? Started { get; }

        public TimeSpan? Duration { get; }

        public int? KeyCount { get; }

        public static UtteranceData FromLine(string line)
        {
            UtteranceData value;

            var input = new StringReader(line);

            using (var reader = XmlReader.Create(input, XmlHelper.ReaderSettings))
            {
                reader.Read();

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "U")
                {
                    DateTimeOffset? started = null;
                    TimeSpan? duration = null;
                    int? keyCount = null;

                    for (var work = reader.MoveToFirstAttribute(); work; work = reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case nameof(Started):
                                started = DateTimeOffset.Parse(reader.Value, null, DateTimeStyles.RoundtripKind);
                                break;

                            case nameof(Duration):
                                duration = TimeSpan.FromMilliseconds(double.Parse(reader.Value));
                                break;

                            case nameof(KeyCount):
                                keyCount = int.Parse(reader.Value);
                                break;
                        }
                    }
                    reader.Read();

                    var sequence = TileSequence.FromEncoded(reader, XmlNodeType.EndElement);
                    reader.Read();

                    reader.ValidateNodeType(XmlNodeType.None);

                    value = new UtteranceData(sequence, started, duration, keyCount);
                }
                else
                {
                    var sequence = TileSequence.FromEncoded(line);

                    value = new UtteranceData(sequence);
                }

            }

            return value;
        }

        public string ToLine()
        {
            string value;

            var content = Sequence.ToHybridEncoded();

            if (Started == null && Duration == null && KeyCount == 0)
            {
                value = content;
            }
            else
            {
                value = XmlHelper.WriteXmlFragment(writer =>
                {
                    writer.WriteStartElement("U");
                    if (Started.HasValue)
                    {
                        writer.WriteAttributeString(nameof(Started), Started.Value.ToString("o"));
                    }
                    if (Duration.HasValue)
                    {
                        writer.WriteAttributeString(nameof(Duration), Duration.Value.TotalMilliseconds.ToString());
                    }
                    if (KeyCount.HasValue)
                    {
                        writer.WriteAttributeString(nameof(KeyCount), KeyCount.Value.ToString());
                    }

                    writer.WriteRaw(content);

                    writer.WriteEndElement();
                });
            }

            return value;
        }
    }
}

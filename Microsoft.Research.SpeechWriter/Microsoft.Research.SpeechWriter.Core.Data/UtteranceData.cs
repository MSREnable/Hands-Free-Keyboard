using System;
using System.Globalization;
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
            IsArtificial = false;
            Started = started;
            Duration = duration;
            KeyCount = keyCount;
        }

        public UtteranceData(TileSequence sequence, bool isArtificial)
        {
            Sequence = sequence;
            IsArtificial = isArtificial;
        }

        public TileSequence Sequence { get; }

        public bool IsArtificial { get; }

        public DateTimeOffset? Started { get; }

        public TimeSpan? Duration { get; }

        public int? KeyCount { get; }

        public static UtteranceData FromLine(string line) => line.ReadXmlFragment<UtteranceData>(reader =>
        {
            UtteranceData value;

            reader.ValidatedRead();

            if (reader.NodeType == XmlNodeType.Element && reader.Name == "U")
            {
                var isArtificial = false;
                DateTimeOffset? started = null;
                TimeSpan? duration = null;
                int? keyCount = null;

                for (var work = reader.MoveToFirstAttribute(); work; work = reader.MoveToNextAttribute())
                {
                    switch (reader.Name)
                    {
                        case nameof(IsArtificial):
                            isArtificial = true;
                            break;

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
                reader.ValidatedRead();

                var sequence = TileSequence.FromEncoded(reader, XmlNodeType.EndElement);
                reader.ReadEndOfFragment();

                value = isArtificial ? new UtteranceData(sequence, true) : new UtteranceData(sequence, started, duration, keyCount);
            }
            else
            {
                var sequence = TileSequence.FromEncoded(line);

                value = new UtteranceData(sequence, false);
            }

            return value;
        });


        public string ToLine()
        {
            var content = Sequence.ToHybridEncoded();

            var value = XmlHelper.WriteXmlFragment(writer =>
            {
                writer.WriteStartElement("U");
                if (IsArtificial)
                {
                    writer.WriteAttributeString(nameof(IsArtificial), IsArtificial.ToString());
                }
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

            return value;
        }
    }
}

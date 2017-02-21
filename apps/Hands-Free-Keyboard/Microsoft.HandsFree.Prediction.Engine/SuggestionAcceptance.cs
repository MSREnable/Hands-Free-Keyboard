namespace Microsoft.HandsFree.Prediction.Engine
{
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Record of an accepted word suggestion.
    /// </summary>
    [XmlType("Accept")]
    public class SuggestionAcceptance
    {
        /// <summary>
        /// UtcNow ticks value. The time the suggestion was actually accepted.
        /// </summary>
        [XmlAttribute]
        public long UtcNow { get; set; }

        /// <summary>
        /// The system tick count. This will correspond to the recorded action that caused the acceptance.
        /// </summary>
        [XmlAttribute]
        public int TickCount { get; set; }

        /// <summary>
        /// The number of letters typed to obtain the suggestion.
        /// </summary>
        [XmlAttribute]
        public int Seed { get; set; }

        /// <summary>
        /// The suggestion's position in the presented list of suggestions.
        /// </summary>
        [XmlAttribute]
        public int Index { get; set; }

        /// <summary>
        /// The bare word being suggested.
        /// </summary>
        [XmlAttribute]
        public string Word { get; set; }
    }
}

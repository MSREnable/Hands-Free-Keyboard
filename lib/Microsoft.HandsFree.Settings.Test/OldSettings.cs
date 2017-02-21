namespace Microsoft.HandsFree.Settings.Test
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Settings")]
    public class OldSettings
    {
        internal static readonly XmlSerializer Serializer = new XmlSerializer(typeof(OldSettings));

        [XmlAttribute]
        public int AlwaysHere { get; set; }

        [XmlAttribute]
        public int LegacyValue { get; set; }
    }
}

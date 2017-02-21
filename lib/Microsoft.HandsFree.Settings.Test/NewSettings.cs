namespace Microsoft.HandsFree.Settings.Test
{
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Settings")]
    public class NewSettings
    {
        internal static readonly XmlSerializer Serializer = new XmlSerializer(typeof(NewSettings));

        [XmlAttribute]
        public int AlwaysHere { get; set; }

        [XmlAttribute]
        public int IntroducedValue { get; set; }
    }
}

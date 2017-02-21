namespace Microsoft.HandsFree.Prediction.Engine
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    public class Spoken
    {
        [XmlAttribute]
        public long UtcNowTicks { get; set; }

        [XmlAttribute]
        public int TickCount { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsInPrivate { get; set; }

        [XmlAttribute]
        public string Text { get; set; }
    }
}

namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    using System.Xml.Serialization;

    public class DemoPayload
    {
        [XmlAttribute]
        public string Text { get; set; }

        [XmlAttribute]
        public int Number { get; set; }
    }
}

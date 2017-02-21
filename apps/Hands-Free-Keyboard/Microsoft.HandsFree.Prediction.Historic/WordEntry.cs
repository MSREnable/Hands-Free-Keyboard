using System.IO;

namespace Microsoft.HandsFree.Prediction.Historic
{
    class WordEntry
    {
        internal string Word { get; set; }

        internal byte UseCount { get; set; }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(Word);
            writer.Write(UseCount);
        }

        internal static WordEntry Read(BinaryReader reader)
        {
            var word = reader.ReadString();
            var useCount = reader.ReadByte();
            var entry = new WordEntry { Word = word, UseCount = useCount };
            return entry;
        }
    }
}

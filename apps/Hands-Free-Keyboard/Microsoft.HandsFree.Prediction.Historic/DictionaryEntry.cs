using System;
using System.IO;

namespace Microsoft.HandsFree.Prediction.Historic
{
    class DictionaryEntry
    {
        internal KeyScoreOrderedList predictions;
        internal int[] key;

        internal void Write(BinaryWriter writer)
        {
            predictions.Write(writer);

            writer.Write(key.Length);
            foreach (var keyPart in key)
            {
                writer.Write(keyPart);
            }
        }

        internal static DictionaryEntry Read(BinaryReader reader)
        {
            var predictions = KeyScoreOrderedList.Read(reader);

            var keyLength = reader.ReadInt32();
            var key = new int[keyLength];
            for (var keyIndex = 0; keyIndex < keyLength; keyIndex++)
            {
                key[keyIndex] = reader.ReadInt32();
            }

            var entry = new DictionaryEntry { predictions = predictions, key = key };

            return entry;
        }
    }
}

using System.Diagnostics;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Database
{
    internal class CompressedIntReader
    {
        private readonly Stream _stream;

        public CompressedIntReader(Stream stream)
        {
            _stream = stream;
        }

        public uint ReadUInt()
        {
            var value = 0u;

            var shift = 0;
            var bits = _stream.ReadByte();
            while (0x80 <= bits)
            {
                value |= (uint)((bits & 0x7F) << shift);
                shift += 7;
                bits = _stream.ReadByte();
            }
            value |= (uint)(bits << shift);

            return value;
        }

        public int ReadInt() => (int)ReadUInt();

        public uint[] ReadUIntArray(params uint[] values)
        {
            var length = ReadInt();
            var value = new uint[length];

            for (var index = 0; index < length; index++)
            {
                value[index] = ReadUInt();
            }

            return value;
        }

        public int[] ReadIntArray()
        {
            var length = ReadInt();
            var value = new int[length];

            for (var index = 0; index < length; index++)
            {
                value[index] = ReadInt();
            }

            return value;
        }

        public char ReadChar() => (char)ReadInt();

        public char[] ReadCharArray()
        {
            var length = ReadInt();
            var value = new char[length];

            for (var index = 0; index < length; index++)
            {
                value[index] = (char)ReadUInt();
            }

            return value;
        }

        public string ReadString()
        {
            var values = ReadCharArray();
            var value = new string(values);
            return value;
        }
    }
}

using System.Diagnostics;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Database
{
    internal class CompressedIntWriter
    {
        private readonly Stream _stream;

        public CompressedIntWriter(Stream stream)
        {
            _stream = stream;
        }

        public void Write(uint value)
        {
            var bits = value;

            while (0x80 <= bits)
            {
                _stream.WriteByte((byte)(0x80 | bits));
                bits >>= 7;
            }

            Debug.Assert(bits < 0x80);
            _stream.WriteByte((byte)bits);
        }

        public void Write(int value) => Write((uint)value);

        public void Write(params uint[] values)
        {
            Write(values.Length);
            foreach (var value in values)
            {
                Write(value);
            }
        }

        public void Write(params int[] values)
        {
            Write(values.Length);
            foreach (var value in values)
            {
                Write(value);
            }
        }

        public void Write(char value) => Write((uint)value);

        public void Write(char[] values)
        {
            Write(values.Length);
            foreach (var value in values)
            {
                Write(value);
            }
        }

        public void Write(string value)
        {
            var values = value.ToCharArray();
            Write(values);
        }
    }
}

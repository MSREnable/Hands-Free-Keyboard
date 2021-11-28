using System.Diagnostics;
using System.IO;

namespace Microsoft.Research.SpeechWriter.Core.Data
{
    public static class TileTypeHelper
    {
        private const string NormalElement = "T";
        private const string PrefixElement = "B";
        private const string SuffixElement = "A";
        private const string InfixElement = "J";
        private const string ExtensionElement = "X";
        private const string CommandElement = "C";

        public static bool IsPrefix(this TileType type)
        {
            return type == TileType.Prefix || type == TileType.Infix;
        }

        public static bool IsSuffix(this TileType type)
        {
            return type == TileType.Suffix || type == TileType.Infix;
        }

        public static TileType FromFixes(bool isPrefix = false, bool isSuffix = false)
        {
            TileType value;

            if (isPrefix)
            {
                if (isSuffix)
                {
                    value = TileType.Infix;
                }
                else
                {
                    value = TileType.Prefix;
                }
            }
            else
            {
                if (isSuffix)
                {
                    value = TileType.Suffix;
                }
                else
                {
                    value = TileType.Normal;
                }
            }

            return value;
        }

        public static string ToElementName(this TileType type)
        {
            string value;

            switch (type)
            {
                case TileType.Normal:
                    value = NormalElement;
                    break;

                case TileType.Suffix:
                    value = SuffixElement;
                    break;

                case TileType.Prefix:
                    value = PrefixElement;
                    break;

                case TileType.Infix:
                    value = InfixElement;
                    break;

                case TileType.Extension:
                    value = ExtensionElement;
                    break;

                case TileType.Command:
                default:
                    Debug.Assert(type == TileType.Command);
                    value = CommandElement;
                    break;
            }

            return value;
        }

        public static TileType FromElementName(string name)
        {
            TileType value;

            switch (name)
            {
                case NormalElement:
                    value = TileType.Normal;
                    break;

                case SuffixElement:
                    value = TileType.Suffix;
                    break;

                case PrefixElement:
                    value = TileType.Prefix;
                    break;

                case InfixElement:
                    value = TileType.Infix;
                    break;

                case ExtensionElement:
                    value = TileType.Extension;
                    break;

                case CommandElement:
                    value = TileType.Command;
                    break;

                default:
                    throw new InvalidDataException();
            }

            return value;
        }
    }
}

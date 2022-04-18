using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Unicoder
{
    internal class Program
    {
        [DllImport("getuname.dll", SetLastError = true)]
        static extern int GetUName(UInt16 wCharCode, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpbuf);

        static void Main(string[] args)
        {
            var charToName = new Dictionary<char, string>();
            var nameToChar = new SortedDictionary<string, char>();

            for (ushort i = 0; i < 0xFFFF; i++)
            {
                var category = char.GetUnicodeCategory((char)i);
                switch (category)
                {
                    case UnicodeCategory.OtherNotAssigned:
                    case UnicodeCategory.Control:
                        break;

                    default:
                        var builder = new StringBuilder(1024);
                        GetUName(i, builder);
                        var name = builder.ToString();

                        if (nameToChar.TryGetValue(name, out var other))
                        {
                            charToName.Remove(other);
                        }
                        else
                        {
                            nameToChar.Add(name, (char)i);
                            charToName.Add((char)i, name);

                            Console.WriteLine($"{i} => {name} ({category})");
                            //Debug.Assert(builder.ToString() != "Undefined");
                        }
                        break;
                }
            }
            Console.ReadKey();
        }
    }
}

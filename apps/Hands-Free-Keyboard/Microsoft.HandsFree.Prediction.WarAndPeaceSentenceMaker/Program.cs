using Microsoft.HandsFree.Prediction.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Microsoft.HandsFree.Prediction.WarAndPeaceSentenceMaker
{
    class Program
    {
        static readonly List<Spoken> records = new List<Spoken>();

        static void ReadToFirstChapterHeader(TextReader reader)
        {
            var found = false;
            var blankCount = 0;
            for (var line = reader.ReadLine(); !found && line != null; line = reader.ReadLine())
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    blankCount++;
                }
                else
                {
                    if (2 < blankCount && line == "CHAPTER I")
                    {
                        found = true;
                    }
                    blankCount = 0;
                }
            }
        }

        static void ProcessText(TextReader reader)
        {
            ReadToFirstChapterHeader(reader);

            var endOfText = false;
            var paragraph = new StringBuilder();
            var blankCount = 0;
            for (var line = reader.ReadLine(); !endOfText && line != null; line = reader.ReadLine())
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (paragraph.Length != 0)
                    {
                        var text = paragraph.ToString();

                        for (var startReference = text.IndexOf('*'); 0 <= startReference; startReference = text.IndexOf('*'))
                        {
                            var endReference = startReference;
                            do
                            {
                                endReference++;
                            }
                            while (endReference < text.Length && text[endReference - 1] != ' ');

                            Console.WriteLine(text.Substring(startReference, endReference - startReference));
                            Console.WriteLine("  " + text.Substring(startReference));

                            text = text.Substring(0, startReference) + text.Substring(endReference);
                        }

                        var record = new Spoken { UtcNowTicks = DateTime.UtcNow.Ticks, TickCount = Environment.TickCount, Text = text };
                        records.Add(record);

                        paragraph.Clear();
                    }

                    blankCount++;
                }
                else
                {
                    switch (blankCount)
                    {
                        case 0:
                            if (paragraph.Length != 0)
                            {
                                paragraph.Append(' ');
                            }
                            paragraph.Append(line);
                            break;
                        case 1:
                            if (line.StartsWith("*"))
                            {
                                Debug.Assert(true);
                            }
                            else if (line.StartsWith("BOOK"))
                            {
                                Debug.Assert(true);
                            }
                            else
                            {
                                Debug.Assert(paragraph.Length == 0);

                                paragraph.Append(line);
                            }
                            break;
                        case 2:
                            if (line.StartsWith("*"))
                            {
                                Debug.Assert(true);
                            }
                            else
                            {
                                Debug.Assert(paragraph.Length == 0);

                                paragraph.Append(line);
                            }
                            break;
                        case 4:
                            Debug.Assert(line.StartsWith("CHAPTER"));
                            break;
                        case 5:
                            endOfText = true;
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                    blankCount = 0;
                }
            }
        }

        static void Main(string[] args)
        {
            var stream = new WebClient().OpenRead("http://www.gutenberg.org/cache/epub/2600/pg2600.txt");
            //var stream = File.OpenRead(@"C:\Users\petea\Desktop\pg2600.txt");

            using (var reader = new StreamReader(stream))
            {
                ProcessText(reader);
            }

            var builder = new StringBuilder();
            foreach (var record in records)
            {
                var fragment = XmlFragmentHelper.EncodeXmlFragment(record);
                builder.AppendLine(fragment);
            }
            File.WriteAllText("WarAndPeace.xml", builder.ToString());
        }
    }
}

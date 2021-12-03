using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace EpubReader
{
    public class EpubStreamHelper
    {
        public static IEnumerable<string> StreamToSentences(Stream stream)
        {
            var archive = new ZipArchive(stream);

            var fullPath = GetFullPath(archive);

            var itemRefs = GetSpine(archive, fullPath);

            var paragraphs = GetParagraphs(archive, fullPath, itemRefs);

            var sentences = GetSentences(paragraphs);

            return sentences;
        }

        private static IEnumerable<string> _standardAbbreviation = new[] { "Mr", "Mrs", "Ms" };

        private static HashSet<char> _quotes = new HashSet<char>() { '\'', '"', '”', '’', '”', ')' };
        private static HashSet<char> _continuations = new HashSet<char>() { '-', '—', ',', ';' };

        private static IEnumerable<string> SplitIntoSentences(string originalParagraph)
        {
            var paragraph = originalParagraph;
            var searchStart = 0;
            var sentenceStart = 0;
            for (var separator = paragraph.IndexOfAny(new[] { '.', '!', '?' }, searchStart);
                separator != -1;
                separator = paragraph.IndexOfAny(new[] { '.', '!', '?' }, searchStart))
            {
                if (separator + 1 == paragraph.Length)
                {
                    var sentence = paragraph.Substring(sentenceStart).Trim();
                    yield return sentence;
                    searchStart = separator + 1;
                    sentenceStart = searchStart;
                }
                else
                {
                    var isAbbreviation = false;
                    if (paragraph[separator] == '.')
                    {
                        using (var enumerator = _standardAbbreviation.GetEnumerator())
                        {
                            while (!isAbbreviation && enumerator.MoveNext())
                            {
                                if (enumerator.Current.Length <= separator)
                                {
                                    var text = paragraph.Substring(separator - enumerator.Current.Length, enumerator.Current.Length);
                                    if (text == enumerator.Current)
                                    {
                                        isAbbreviation = true;
                                    }
                                }
                            }
                        }
                    }

                    if (isAbbreviation)
                    {
                        // Remove unwanted period.
                        //paragraph = paragraph.Substring(0, separator) + paragraph.Substring(separator + 1);
                        searchStart = separator + 1;
                    }
                    else if (char.IsLetterOrDigit(paragraph[separator + 1]))
                    {
                        // Continue if we've got an embedded period.
                        searchStart = separator + 1;
                    }
                    else
                    {
                        var afterQuotes = separator + 1;
                        while (afterQuotes < paragraph.Length && _quotes.Contains(paragraph[afterQuotes]))
                        {
                            afterQuotes++;
                        }

                        if (afterQuotes < paragraph.Length)
                        {
                            if (paragraph[afterQuotes] == ' ')
                            {
                                if (afterQuotes + 1 < paragraph.Length && char.IsLower(paragraph[afterQuotes + 1]))
                                {
                                    searchStart = afterQuotes + 2;
                                }
                                else
                                {
                                    var sentence = paragraph.Substring(sentenceStart, afterQuotes - sentenceStart).Trim();
                                    yield return sentence;
                                    searchStart = afterQuotes + 1;
                                    sentenceStart = searchStart;
                                }
                            }
                            else if (_continuations.Contains(paragraph[afterQuotes]))
                            {
                                searchStart = afterQuotes + 1;
                            }
                            else
                            {
                                Debug.WriteLine($"{paragraph.Substring(0, afterQuotes)} <--> {paragraph.Substring(afterQuotes)}");
                                Debugger.Break();
                                searchStart = afterQuotes + 1;
                            }
                        }
                        else
                        {
                            var sentence = paragraph.Substring(sentenceStart).Trim();
                            yield return sentence;
                            searchStart = paragraph.Length;
                            sentenceStart = searchStart;
                        }
                    }
                }
            }

            var lastSentence = paragraph.Substring(sentenceStart).Trim();
            if (lastSentence.Length != 0)
            {
                yield return lastSentence;
            }
        }

        private static IEnumerable<string> GetSentences(IEnumerable<string> paragraphs)
        {
            foreach (var paragraph in paragraphs)
            {
                var sentences = SplitIntoSentences(paragraph);
                foreach (var sentence in sentences)
                {
                    yield return sentence;
                }
            }
        }

        private static IEnumerable<string> GetParagraphs(ZipArchive archive, string fullPath, List<string> itemRefs)
        {
            var itemFolder = fullPath.Substring(0, fullPath.LastIndexOf('/') + 1);
            foreach (var itemRef in itemRefs)
            {
                var fullItemPath = itemFolder + itemRef;
                var entry = archive.GetEntry(fullItemPath);
                using (var entryStream = entry.Open())
                {
                    var xml = XDocument.Load(entryStream);
                    var root = xml.Root;
                    var space = root.GetDefaultNamespace();
                    XName NS(string localName) => space.GetName(localName);

                    var body = root.Element(NS("body"));
                    foreach (var paragraph in body?.Descendants(NS("p")))
                    {
                        yield return paragraph.Value;
                    }
                }
            }
        }

        private static List<string> GetSpine(ZipArchive archive, string fullPathValue)
        {
            List<string> itemRefs = new List<string>();
            var rootfileEntry = archive.GetEntry(fullPathValue);
            using (var rootfileStream = rootfileEntry.Open())
            {
                var xml = XDocument.Load(rootfileStream);
                var root = xml.Root;
                var space = root.GetDefaultNamespace();
                XName NS(string localName) => space.GetName(localName);

                var idToHref = new Dictionary<string, string>();
                var manifest = root.Element(NS("manifest"));
                foreach (var item in manifest.Elements(NS("item")))
                {
                    if (item.Attribute("media-type").Value == "application/xhtml+xml")
                    {
                        var href = item.Attribute("href").Value;
                        var id = item.Attribute("id").Value;
                        idToHref.Add(id, href);
                    }
                }

                var spine = root.Element(NS("spine"));
                foreach (var itemref in spine.Elements(NS("itemref")))
                {
                    var idref = itemref.Attribute("idref").Value;
                    if (idToHref.TryGetValue(idref, out var href))
                    {
                        itemRefs.Add(href);
                    }
                }
            }

            return itemRefs;
        }

        private static string GetFullPath(ZipArchive archive)
        {
            string fullPathValue;
            var containerEntry = archive.GetEntry("META-INF/container.xml");
            using (var containerStream = containerEntry.Open())
            {
                var xml = XDocument.Load(containerStream);
                var root = xml.Root;
                var space = root.GetDefaultNamespace();
                XName NS(string localName) => space.GetName(localName);
                var rootfiles = root.Element(NS("rootfiles"));
                var rootfile = rootfiles.Element(NS("rootfile"));
                var fullPath = rootfile.Attribute("full-path");
                fullPathValue = fullPath.Value;
            }

            return fullPathValue;
        }
    }
}

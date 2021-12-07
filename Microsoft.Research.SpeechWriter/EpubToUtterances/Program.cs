using EpubReader;
using Microsoft.Research.SpeechWriter.Core.Data;
using System;
using System.IO;

namespace EpubToUtterances
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];
            using (var stream = File.OpenRead(path))
            {
                var sentences = EpubStreamHelper.StreamToSentences(stream);

                foreach (var sentence in sentences)
                {
                    var sequence = TileSequence.FromRaw(sentence);
                    var utterance = new UtteranceData(sequence, true);
                    var line = utterance.ToLine();
                    Console.WriteLine(line);
                }
            }
        }
    }
}

using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Lucene.Internals;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.HandsFree.Prediction.Lucene
{
    public static class WordIndexFactory
    {
        const double MinDictionaryScore = 0.5;
        const double MaxDictionaryScore = 10;
        const double MinUserScore = MaxDictionaryScore;
        const double MaxUserScore = 20;

        const int FileHeaderMagic = 43243214;

        static int ExpectedHistoryHash = string.Empty.GetHashCode();

        public static void ExtendWithDictinoaryWords(Dictionary<string, float> vocabulary, IEnumerable<WordScorePair> wordScorePairs)
        {
            var wordToDictionaryScore = new Dictionary<string, double>();
            var maxScore = 0.0;

            foreach (var pair in wordScorePairs)
            {
                var mixedCaseWord = pair.Word;

                if (!((IEnumerable<char>)mixedCaseWord).Any(c => !char.IsLetter(c)) && !((IEnumerable<char>)mixedCaseWord).All(c => char.IsUpper(c)))
                {
                    var word = mixedCaseWord;

                    Debug.Assert(!wordToDictionaryScore.ContainsKey(word), "Duplicate words in dictionary");

                    var score = pair.Score;

                    Debug.Assert(!wordToDictionaryScore.ContainsKey(word));
                    wordToDictionaryScore.Add(word, score);

                    if (maxScore < score)
                    {
                        maxScore = score;
                    }
                }
            }

            foreach (var pair in wordToDictionaryScore)
            {
                var boost = (float)(MinDictionaryScore + (MaxDictionaryScore - MinDictionaryScore) * pair.Value / maxScore);
                vocabulary.Add(pair.Key, boost);
            }
        }

        static Dictionary<string, float> CreateVocabularyDictionaryWithCounts(IEnumerable<WordScorePair> wordScorePairs)
        {
            var vocabulary = new Dictionary<string, float>();

            ExtendWithDictinoaryWords(vocabulary, wordScorePairs);

            return vocabulary;
        }

        static RAMDirectory CreateIndex(Dictionary<string, float> vocabulary, Analyzer analyzer)
        {
            var index = new RAMDirectory();

            using (var writer = new IndexWriter(index, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                foreach (var pair in vocabulary)
                {
                    var document = new Document();
                    var field = new Field(WordIndex.WordFieldName, WordIndex.LeadCharacter + pair.Key + WordIndex.TailCharacter, Field.Store.YES, Field.Index.ANALYZED);
                    document.Boost = pair.Value;
                    document.Add(field);
                    writer.AddDocument(document);
                }
            }

            return index;
        }

        static RAMDirectory LoadCache(IPredictionEnvironment environment, int expectedPairsHash, int expectedHistoryHash)
        {
            RAMDirectory directory = null;

            using (var reader = environment.OpenStaticDictionaryCache())
            {
                try
                {
                    var actualFileHeaderMagic = reader.ReadInt32();
                    var actualPairsHash = reader.ReadInt32();
                    var actualHistoryHash = reader.ReadInt32();

                    if (FileHeaderMagic == actualFileHeaderMagic && expectedPairsHash == actualPairsHash && expectedHistoryHash == actualHistoryHash)
                    {
                        directory = new RAMDirectory();
                        var nameCount = reader.ReadInt16();

                        for (var nameIndex = 0; nameIndex < nameCount; nameIndex++)
                        {
                            var name = reader.ReadString();
                            var length = reader.ReadInt32();
                            var bytes = reader.ReadBytes(length);

                            using (var output = directory.CreateOutput(name))
                            {
                                output.WriteBytes(bytes, length);
                            }
                        }
                    }
                }
                catch
                {
                    directory = null;
                }
            }

            return directory;
        }

        static void SaveCache(IPredictionEnvironment environment, int expectedPairsHash, int expectedHistoryHash, RAMDirectory directory)
        {
            using (var writer = environment.CreateStaticDictionaryCache())
            {
                writer.Write(FileHeaderMagic);
                writer.Write(expectedPairsHash);
                writer.Write(expectedHistoryHash);

                var names = directory.ListAll();
                writer.Write((System.Int16)names.Length);

                foreach (var name in names)
                {
                    var reader = directory.OpenInput(name);
                    var length = reader.Length();
                    var buffer = new byte[length];
                    reader.ReadBytes(buffer, 0, (int)length);

                    writer.Write(name);
                    writer.Write((System.Int32)length);
                    writer.Write(buffer);
                }
            }
        }

        public static IWordIndex CreateFromWordCountList(IPredictionEnvironment environment, IEnumerable<WordScorePair> wordScorePairs)
        {
            var index = CreateFromWordCountListJava(environment, wordScorePairs);

            return index;
        }

        public static IWordIndex CreateFromWordCountListJava(IPredictionEnvironment environment, IEnumerable<WordScorePair> wordScorePairs)
        {
            var analyzer = WordIndex.CreateAnalyzer();

            var expectedPairsHash = wordScorePairs.GetHashCode();

            var directory = LoadCache(environment, expectedPairsHash, ExpectedHistoryHash);

            if (directory == null)
            {
                var vocabulary = CreateVocabularyDictionaryWithCounts(wordScorePairs);

                directory = CreateIndex(vocabulary, analyzer);
                SaveCache(environment, expectedPairsHash, ExpectedHistoryHash, directory);
            }

            return new WordIndex(analyzer, directory);
        }
    }
}


namespace Microsoft.HandsFree.Prediction.Measurer
{
    using Microsoft.HandsFree.Prediction.Api;
    using Microsoft.HandsFree.Prediction.Engine;
    using Microsoft.HandsFree.Prediction.Measurer.Properties;
    using System;
    using System.IO;

    class Program
    {
        static void DoBackgroundWork(SingleThreadedPredictionEnvironment environment)
        {
            while (environment.workItems.Count != 0)
            {
                var workItem = environment.workItems[0];
                environment.workItems.RemoveAt(0);
                workItem();
            }
        }

        static void Main(string[] args)
        {
            using (var writer = File.CreateText("output.txt"))
            {
                var text = Resources.Script;

                var environment = new SingleThreadedPredictionEnvironment();
                var predictor = PredictionEngineFactory.Create(environment);

                DoBackgroundWork(environment);

                for (var pass = 0; pass < 2; pass++)
                {
                    var sentence = string.Empty;

                    var gainCount = 0;
                    var totalCount = 0;

                    var position = text.PunctuationLength(0);
                    while (position < text.Length)
                    {
                        var wordStart = position;
                        var wordLength = text.WordLength(position);
                        position += wordLength;

                        var word = text.Substring(wordStart, wordLength);

                        var punctuationStart = position;
                        var punctuationLength = text.PunctuationLength(position);
                        position += punctuationLength;

                        var punctuation = text.Substring(punctuationStart, punctuationLength);

                        var initialPrediction = predictor.CreatePrediction(sentence, 0, sentence.Length, false, null);
                        var initialSuggestions = initialPrediction.GetSuggestions(SuggestionType.Word);

                        var suggested = false;

                        using (var enumerator = initialSuggestions.GetEnumerator())
                        {
                            var suggestion = 0;
                            while (!suggested && enumerator.MoveNext())
                            {
                                if (string.Compare(enumerator.Current.Text, word, StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    suggested = true;
                                    enumerator.Current.Accepted(suggestion);
                                }

                                suggestion++;
                            }
                        }

                        var inputCount = 1;

                        if (!suggested)
                        {
                            var prefix = sentence;

                            var keys = 0;
                            while (keys < word.Length && !suggested)
                            {
                                prefix += word[keys];
                                keys++;
                                inputCount++;

                                var wordPrediction = predictor.CreatePrediction(prefix, prefix.Length, 0, false, null);
                                var wordSuggestions = wordPrediction.GetSuggestions(SuggestionType.Word);

                                using (var enumerator = wordSuggestions.GetEnumerator())
                                {
                                    if (enumerator.MoveNext())
                                    {
                                        if (string.Compare(enumerator.Current.Text, word, StringComparison.InvariantCultureIgnoreCase) == 0)
                                        {
                                            suggested = true;
                                            enumerator.Current.Accepted(0);
                                        }
                                    }
                                }
                            }
                        }

                        var textCount = word.Length + 1;
                        gainCount += textCount - inputCount;
                        totalCount += textCount;

                        var line = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", word, textCount, inputCount, gainCount, totalCount, suggested);
                        Console.WriteLine(line);
                        writer.WriteLine(line);

                        if (punctuation.IsSentenceEnding() || position == text.Length)
                        {
                            predictor.RecordHistory(sentence, false);
                            sentence = string.Empty;
                        }
                        else if (sentence.Length == 0)
                        {
                            sentence = word;
                        }
                        else
                        {
                            sentence += " " + word;
                        }

                        DoBackgroundWork(environment);
                    }
                }
            }
        }
    }
}

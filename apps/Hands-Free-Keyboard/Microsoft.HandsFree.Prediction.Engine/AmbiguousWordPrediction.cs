using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HandsFree.Prediction.Api;
using Microsoft.HandsFree.Prediction.Lucene;
using Microsoft.HandsFree.Prediction.Lucene.Internals;

namespace Microsoft.HandsFree.Prediction.Engine
{
    public class AmbiguousWordPrediction : IPrediction
    {
        static Dictionary<string, float> _vocabulary;

        List<IPredictionSuggestion> _suggestions;

        static AmbiguousWordPrediction()
        {
            _vocabulary = new Dictionary<string, float>();
            WordIndexFactory.ExtendWithDictinoaryWords(_vocabulary, WordScorePairEnumerable.Instance);

        }

        public AmbiguousWordPrediction(string text, int selectionStart, int selectionEnd, bool isAutoSpace, List<List<string>> clusterSequence)
        {
            StringBuilder strWorker = new StringBuilder();
            List<WordScorePair> candidates = new List<WordScorePair>();
            GetCandidateWords(clusterSequence, 0, strWorker, candidates);
            candidates = candidates.OrderByDescending(o => o.Score).ToList();

            // drop the last character because we get here after it was added to the text
            text = text.Substring(0, text.Length - 1);
            selectionStart = selectionStart - 1;

            var lastWordLen = text.ReverseWordLength(selectionStart);
            var start = selectionStart - lastWordLen;


            _suggestions = new List<IPredictionSuggestion>();
            foreach (var candidate in candidates)
            {
                var suggestion = new AmbiguousPredictionSuggestion(candidate.Word, candidate.Score, start, true);
                _suggestions.Add(suggestion);
            }

            // Add the characters from the last cluster - except the first character
            var items = clusterSequence[clusterSequence.Count - 1];
            var insertIndex = (_suggestions.Count > 0) ? 1 : 0;
            
            var lastWordFragment = text.Substring(start, lastWordLen);

            for (var i = 1; i < items.Count; i++)
            {
                // Add these words with a trailing underscore to indicate that it is an incomplete word
                var suggestion = new AmbiguousPredictionSuggestion(lastWordFragment + items[i] + '_', 0, start, false);
                _suggestions.Insert(insertIndex++, suggestion);
            }
        }

        void GetCandidateWords(List<List<string>> clusterSequence, int row, StringBuilder strWorker, List<WordScorePair> candidates)
        {
            List<string> clusterChars = clusterSequence[row];
            for (int i = 0; i < clusterChars.Count; i++)
            {
                string str = clusterChars[i];
                strWorker.Append(str);

                if (row < clusterSequence.Count - 1)
                {
                    GetCandidateWords(clusterSequence, row + 1, strWorker, candidates);
                }
                else
                {
                    float score;
                    if (_vocabulary.TryGetValue(strWorker.ToString(), out score))
                    {
                        candidates.Add(new WordScorePair(strWorker.ToString(), score));
                    }
                }

                // Remove the last element from the worker string
                strWorker.Remove(strWorker.Length - str.Length, str.Length);
            }
        }
        public IPredictionSuggestionCollection GetSuggestions(SuggestionType type)
        {
            return new AmbiguousPredictionCollection(_suggestions);
        }
    }

    public class AmbiguousPredictionCollection : IPredictionSuggestionCollection
    {
        List<IPredictionSuggestion> _suggestions;
        public AmbiguousPredictionCollection(List<IPredictionSuggestion> suggestions)
        {
            _suggestions = suggestions;
        }
        public string[] Context { get { return null; } }

        public IEnumerator<IPredictionSuggestion> GetEnumerator()
        {
            return _suggestions.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _suggestions.GetEnumerator();
        }
    }

    public class AmbiguousPredictionSuggestion : IPredictionSuggestion
    {
        public AmbiguousPredictionSuggestion(string word, double score, int start, bool completeWord)
        {
            Confidence = score;
            Text = word;
            ReplacementStart = start;
            ReplacementLength = completeWord ? Text.Length : Text.Length - 1;
            CompleteWord = completeWord;
        }

        public double Confidence { get; }

        public string Text { get; }

        public int ReplacementStart { get; }

        public int ReplacementLength { get; }

        public bool CompleteWord { get; }

        public void Accepted(int index)
        {
            Debug.WriteLine($"Accepted: {index}");
        }
    }
}

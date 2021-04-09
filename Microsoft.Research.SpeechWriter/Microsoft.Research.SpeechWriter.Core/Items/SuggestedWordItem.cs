﻿using System;

namespace Microsoft.Research.SpeechWriter.Core.Items
{
    /// <summary>
    /// A word in the suggestion list.
    /// </summary>
    public class SuggestedWordItem : WordItem, ISuggestionItem
    {
        private readonly SuggestedWordItem _previous;

        internal SuggestedWordItem(WordVocabularySource source, SuggestedWordItem previous, string word)
            : base(source, word)
        {
            _previous = previous;
        }

        internal SuggestedWordItem(WordVocabularySource source, string word)
            : this(source, null, word)
        {
        }

        internal string[] Words
        {
            get
            {
                string[] value;

                if (_previous != null)
                {
                    value = _previous.Words;
                    var index = value.Length;
                    Array.Resize(ref value, index + 1);
                    value[index] = Word;
                }
                else
                {
                    value = new[] { Word };
                }

                return value;
            }
        }

        internal override void Execute(WordVocabularySource source)
        {
            source.AddSuggestedSequence(Words);
        }

        ISuggestionItem ISuggestionItem.GetNextItem(int token)
        {
            var item = Source.GetNextItem(this, token);
            return item;
        }
    }
}

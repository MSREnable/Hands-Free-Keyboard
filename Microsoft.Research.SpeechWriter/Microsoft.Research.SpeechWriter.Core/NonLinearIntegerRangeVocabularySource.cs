using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class NonLinearIntegerRangeVocabularySource : VocabularySource
    {
        private readonly int _minimum;
        private readonly int _limit;

        internal NonLinearIntegerRangeVocabularySource(ApplicationModel model, int minimum, int limit)
            : base(model)
        {
            _minimum = minimum;
            _limit = limit;
        }

        internal sealed override int Count => _limit - _minimum;

        internal sealed override IReadOnlyList<ITile> CreateSuggestionList(int index)
        {
            var item = CreateItem(_minimum + index);
            var list = new[] { item };
            return list;
        }

        internal abstract ITile CreateItem(int value);

        private static IEnumerable<double> BisectingSequence
        {
            get
            {
                for (var denominator = 2.0; ; denominator += denominator)
                {
                    for (var numerator = 1.0; numerator < denominator; numerator += 2.0)
                    {
                        var fraction = numerator / denominator;
                        yield return fraction;
                    }
                }
            }
        }

        private static IEnumerable<double> LogBisectingSequence
        {
            get
            {
                foreach (var linear in BisectingSequence)
                {
                    var log = 1.0 - Math.Log10(1.0 + 9 * linear);
                    yield return log;
                }
            }
        }

        internal override IEnumerable<int> GetTopIndices(int minIndex, int limIndex, int count)
        {
            using (var enumerator = LogBisectingSequence.GetEnumerator())
            {
                var span = limIndex - minIndex;
                var effectiveCount = Math.Min(span, count);

                var register = new HashSet<int>();

                for (var i = 0; i < effectiveCount; i++)
                {
                    enumerator.MoveNext();
                    var seed = enumerator.Current;
                    var candidateRealIndex = span * seed;
                    var value = (int)Math.Floor(candidateRealIndex);

                    var fix = 0;

                    while (value < effectiveCount - 1 && register.Contains(value))
                    {
                        value++;
                        fix++;
                    }

                    while (register.Contains(value))
                    {
                        value--;
                        fix--;
                    }

                    register.Add(value);

                    yield return minIndex + value;
                }
            }
        }
    }
}

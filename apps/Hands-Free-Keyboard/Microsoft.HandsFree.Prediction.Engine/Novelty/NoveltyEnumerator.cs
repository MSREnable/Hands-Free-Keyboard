using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Prediction.Engine.Novelty
{
    using Microsoft.HandsFree.Prediction.Api;
    using Settings;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    class NoveltyEnumeratable<T> : Enumerable<T>
        where T : class
    {
        readonly IEnumerable<T> innerEnumerable;

        readonly Func<T, bool> noveltyFunction;

        public NoveltyEnumeratable(IEnumerable<T> innerEnumerable, Func<T, bool> noveltyFunction)
        {
            this.innerEnumerable = innerEnumerable;
            this.noveltyFunction = noveltyFunction;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            if (AppSettings.Instance.Prediction.PredictionNovelty != PredictionNovelty.Never)
            {
                using (var enumerator = innerEnumerable.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        // We have content to consider.

                        // Sneaky peek at the next item.
                        var current = enumerator.Current;
                        Debug.Assert(current != null, "null is not a valid enumable value");

                        if (!noveltyFunction(current))
                        {
                            // We have non-novel data to consider.

                            // Create a list of the leading dull data.
                            var boringList = new List<T>();
                            do
                            {
                                boringList.Add(current);
                                if (enumerator.MoveNext())
                                {
                                    current = enumerator.Current;
                                    Debug.Assert(current != null, "null is not a valid enumable value");
                                }
                                else
                                {
                                    current = null;
                                }
                            }
                            while (current != null && !noveltyFunction(current));

                            if (current != null)
                            {
                                // We arrived at a novel item, so show it first.
                                yield return current;
                            }

                            // Enumerate the data we'd skipped.
                            foreach (var boring in boringList)
                            {
                                yield return boring;
                            }
                        }
                        else
                        {
                            // Yield the data we peeked at.
                            yield return current;
                        }

                        // Enumerate the rest of the data.
                        while (enumerator.MoveNext())
                        {
                            yield return enumerator.Current;
                        }
                    }
                }
            }
            else
            {
                foreach (var value in innerEnumerable)
                {
                    yield return value;
                }
            }
        }
    }
}

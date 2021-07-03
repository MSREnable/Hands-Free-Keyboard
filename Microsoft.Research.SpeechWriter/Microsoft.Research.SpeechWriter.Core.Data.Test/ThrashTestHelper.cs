using System;

namespace Microsoft.Research.SpeechWriter.Core.Data.Test
{
    internal static class ThrashTestHelper
    {
        public static void Run<T>(int length, Action<T[]> test, params T[] items)
        {
            var indices = new int[length];
            var testParams = new T[length];

            var itemsLength = items.Length;

            var done = false;
            while (!done)
            {
                for (var i = 0; i < length; i++)
                {
                    testParams[i] = items[indices[i]];
                }

                test(testParams);

                done = true;
                for (var i = 0; i < length && done; i++)
                {
                    var index = indices[i];
                    index++;
                    if (index == itemsLength)
                    {
                        index = 0;
                    }
                    else
                    {
                        done = false;
                    }
                    indices[i] = index;
                }
            }
        }
    }
}

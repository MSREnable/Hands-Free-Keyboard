using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.Core
{
    internal interface IPredictableNode<T>
        where T : IPredictableNode<T>
    {
        int Count { get; }

        IEnumerable<T> GetChildren();
    }
}

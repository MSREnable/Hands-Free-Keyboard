namespace Microsoft.HandsFree.Prediction.Api
{
    using System.Collections;
    using System.Collections.Generic;

    public abstract class Enumerable<T> : IEnumerable<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

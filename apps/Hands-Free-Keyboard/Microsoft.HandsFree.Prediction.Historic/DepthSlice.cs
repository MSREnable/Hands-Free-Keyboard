namespace Microsoft.HandsFree.Prediction.Historic
{
    /// <summary>
    /// Description of slice through a depth mapping.
    /// </summary>
    public struct DepthSlice
    {
        internal DepthSlice(int start, int limit)
            : this()
        {
            Start = start;
            Limit = limit;
        }

        /// <summary>
        /// First index of slice.
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// First index beyond end of slice.
        /// </summary>
        public int Limit { get; private set; }
    }
}

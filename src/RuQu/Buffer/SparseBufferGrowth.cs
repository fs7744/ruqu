using System.Diagnostics;

namespace RuQu.Buffer
{
    public enum SparseBufferGrowth
    {
        /// <summary>
        /// Each memory chunk has identical size.
        /// </summary>
        None = 0,

        /// <summary>
        /// The size of the new memory chunk is a multiple of the chunk index.
        /// </summary>
        Linear = 1,

        /// <summary>
        /// Each new memory chunk doubles in size.
        /// </summary>
        Exponential = 2,
    }

    internal static class SparseBufferGrowthFuncs
    {
        internal static int LinearGrowth(int chunkSize, ref int chunkIndex) => Math.Max(chunkSize * ++chunkIndex, chunkSize);

        internal static int ExponentialGrowth(int chunkSize, ref int chunkIndex) => Math.Max(chunkSize << ++chunkIndex, chunkSize);

        internal static int NoGrowth(int chunkSize, ref int chunkIndex)
        {
            Debug.Assert(chunkIndex == 0);
            return chunkSize;
        }

        internal static unsafe delegate*<int, ref int, int> GetFunc(this SparseBufferGrowth growth) => growth switch
        {
            SparseBufferGrowth.Linear => &LinearGrowth,
            SparseBufferGrowth.Exponential => &ExponentialGrowth,
            _ => &NoGrowth,
        };
    }
}
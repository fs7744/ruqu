using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace RuQu.Buffer
{
    public interface IBuffer<T> : IDisposable
    {
        public int ConsumedCount { get; }
        public int Index { get; }

        public void Consume(int count);

    }


    public class TextSparseBufferReader
    {

        private readonly unsafe delegate*<int, ref int, int> growth;
        private readonly TextReader reader;
        private readonly ArrayPool<char> pool;

        public TextSparseBufferReader(TextReader reader, ArrayPool<char> pool, int chunkSize = 256, SparseBufferGrowth growth = SparseBufferGrowth.None)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
            this.reader = reader;
            this.pool = pool;
            unsafe
            {
                this.growth = growth.GetFunc();
            }    
        }

        public TextSparseBufferReader(TextReader reader, int chunkSize = 256, SparseBufferGrowth growth = SparseBufferGrowth.None) : this(reader, ArrayPool<char>.Shared, chunkSize, growth)
        {
        }
    }
}

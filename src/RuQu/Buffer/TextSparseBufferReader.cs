using System.Buffers;

namespace RuQu.Buffer
{
    public class TextSparseBufferReader : IChunkReader<char>
    {
        private readonly unsafe delegate*<int, ref int, int> growth;
        private readonly TextReader reader;
        internal readonly ArrayPool<char> pool;
        private int chunkSize;
        private int chunkIndex;
        private IChunk<char> currentChunk;

        public bool IsEOF => currentChunk.Next() == null && currentChunk.IsEOF;

        public TextSparseBufferReader(TextReader reader, ArrayPool<char> pool, int chunkSize = 128, SparseBufferGrowth growth = SparseBufferGrowth.None)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
            this.reader = reader;
            this.pool = pool;
            this.chunkSize = chunkSize;
            unsafe
            {
                this.growth = growth.GetFunc();
            }
        }

        public TextSparseBufferReader(TextReader reader, int chunkSize = 128, SparseBufferGrowth growth = SparseBufferGrowth.None) : this(reader, ArrayPool<char>.Shared, chunkSize, growth)
        {
        }

        internal class Chunk : ReadOnlySequenceSegment<char>, IChunk<char>
        {
            private char[] array;
            internal int index;
            private readonly TextSparseBufferReader textSparseBufferReader;

            ReadOnlySpan<char> IChunk<char>.UnreadSpan => base.Memory.Span.Slice(index);

            public int Length => base.Memory.Length;

            public int Index => index;

            public bool IsEOF => index >= base.Memory.Length;

            ReadOnlyMemory<char> IChunk<char>.UnreadMemory => base.Memory.Slice(index);

            public char this[int index]
            {
                get => base.Memory.Span[index];
            }

            public Chunk(char[] array, int count, TextSparseBufferReader textSparseBufferReader)
            {
                base.Memory = array.AsMemory(0, count);
                this.array = array;
                this.textSparseBufferReader = textSparseBufferReader;
            }

            public void Dispose()
            {
                if (array.Length != 0)
                {
                    textSparseBufferReader.pool.Return(array);
                }
            }

            public IChunk<char>? Next()
            {
                if (base.Memory.IsEmpty) return null;
                if (base.Next == null)
                {
                    var r = textSparseBufferReader.NextBuffer();
                    base.Next = r;
                    if (r != null)
                    {
                        r.RunningIndex = RunningIndex + base.Memory.Length;
                    }
                }
                return base.Next as IChunk<char>;
            }

            public void Consume(int count)
            {
                if (index >= base.Memory.Length) return;
                index += count;
                if (index >= base.Memory.Length)
                {
                    if (base.Next is Chunk chunk)
                    {
                        textSparseBufferReader.currentChunk = chunk;
                        chunk.Consume(index - Memory.Length);
                    }
                    index = base.Memory.Length;
                }
            }
        }

        public unsafe IChunk<char> GetCurrentChunk()
        {
            if (currentChunk == null)
            {
                currentChunk = NextBuffer();
                currentChunk ??= new Chunk(Array.Empty<char>(), 0, this);
            }
            return currentChunk;
        }

        internal unsafe Chunk? NextBuffer()
        {
            var buffer = pool.Rent(growth(chunkSize, ref chunkIndex));
            try
            {
                var count = 0;
                do
                {
                    int readCount = reader.Read(buffer.AsSpan(count));
                    if (readCount == 0)
                    {
                        break;
                    }

                    count += readCount;
                }
                while (count < buffer.Length);
                if (count == 0)
                {
                    pool.Return(buffer);
                }
                else
                {
                    return new Chunk(buffer, count, this);
                }
            }
            catch
            {
                pool.Return(buffer);
                throw;
            }
            return null;
        }

        public void Dispose()
        {
            if (currentChunk != null)
            {
                currentChunk.Dispose();
                currentChunk = null;
            }
        }
    }
}
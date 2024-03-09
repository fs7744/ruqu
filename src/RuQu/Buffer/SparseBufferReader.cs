using System.Buffers;

namespace RuQu.Buffer
{
    public interface IChunk<T>: IDisposable
    {
        public ReadOnlySpan<T> Span { get; }
        public ReadOnlyMemory<T> Memory { get; }

        public IChunk<char>? Next();
        public void Consume(int count);
    }

    public class TextSparseBufferReader
    {
        private readonly unsafe delegate*<int, ref int, int> growth;
        private readonly TextReader reader;
        internal readonly ArrayPool<char> pool;
        private int chunkSize;
        private int chunkIndex;
        private IChunk<char> currentChunk;

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

            new ReadOnlyMemory<char> Memory => base.Memory.Slice(index);
            ReadOnlySpan<char> IChunk<char>.Span => base.Memory.Span.Slice(index);

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
                        this.Dispose();
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
                if (currentChunk == null)
                {
                    currentChunk = new Chunk(Array.Empty<char>(), 0, this);
                }
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
    }
}
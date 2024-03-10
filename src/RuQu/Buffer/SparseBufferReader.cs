using RuQu.Reader;
using System.Buffers;

namespace RuQu.Buffer
{
    public interface IChunk<T> : IDisposable
    {
        public ReadOnlySpan<T> Span { get; }
        public ReadOnlyMemory<T> Memory { get; }

        public int Length { get; }

        public int Index { get; }

        public T this[int index] { get; }

        public IChunk<T>? Next();

        public void Consume(int count);
    }

    public interface IChunkReader<T> : IDisposable
    {
        IChunk<T> GetCurrentChunk();
    }

    public class StringSparseBufferReader : IChunkReader<char>, IChunk<char>
    {
        private readonly string data;
        internal int index;

        public StringSparseBufferReader(string data)
        {
            this.data = data;
        }

        public char this[int index]
        {
            get
            {
                return data[index];
            }
        }

        public ReadOnlySpan<char> Span => data.AsSpan(index);

        public ReadOnlyMemory<char> Memory => data.AsMemory(index);

        public int Length => data.Length;

        public int Index => index;

        public void Consume(int count)
        {
            index = Math.Min(count + index, data.Length);
        }

        public void Dispose()
        {
        }

        public IChunk<char> GetCurrentChunk()
        {
            return this;
        }

        public IChunk<char>? Next()
        {
            return null;
        }
    }

    public class TextSparseBufferReader : IChunkReader<char>
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

            private new ReadOnlyMemory<char> Memory => base.Memory.Slice(index);
            ReadOnlySpan<char> IChunk<char>.Span => base.Memory.Span.Slice(index);

            public int Length => base.Memory.Length;

            public int Index => index;

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

    public abstract class ChunkCharParserBase<T>
    {
        public int BufferSize { get; set; } = 256;

        public SparseBufferGrowth Growth { get; set; } = SparseBufferGrowth.Exponential;

        public virtual T? Read(string content)
        {
            var buffer = new StringSparseBufferReader(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(TextReader reader)
        {
            var buffer = new TextSparseBufferReader(reader, BufferSize, Growth);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        protected abstract T? Read(IChunkReader<char> buffer);
    }

    public static class ChunkBufferExtensions
    {
        public static bool TryPeek<T>(this IChunkReader<T> buffer, out T t)
        {
            var chunk = buffer.GetCurrentChunk();
            if (chunk == null || chunk.Length == 0 || chunk.Index >= chunk.Length)
            {
                t = default;
                return false;
            }
            t = chunk[chunk.Index];
            return true;
        }

        public static bool TryPeek<T>(this IChunkReader<T> buffer, int count, out ReadOnlySequence<T> t)
        {
            var chunk = buffer.GetCurrentChunk();
            if (chunk == null || chunk.Length == 0)
            {
                t = default;
                return false;
            }
            var m = chunk.Memory;
            if (count <= m.Length)
            {
                t = new ReadOnlySequence<T>(m[..count]);
                return true;
            }

            var c = count - m.Length;
            IChunk<T> next;
            while (c > 0)
            {
                next = chunk.Next();
                if (next == null)
                {
                    break;
                }
                var memory = next.Memory;
                if (c <= memory.Length)
                {
                    t = new ReadOnlySequence<T>(chunk as ReadOnlySequenceSegment<T>, chunk.Index, next as ReadOnlySequenceSegment<T>, c - 1);
                    return true;
                }
                c -= memory.Length;
            }
            t = default;
            return false;
        }

        public static T Tag<T>(this IChunkReader<T> buffer, T tag) where T : IEquatable<T>
        {
            if (!buffer.TryPeek(out var t))
            {
                throw new ParseException($"Expect {tag} but got eof");
            }

            if (tag.Equals(t))
            {
                buffer.GetCurrentChunk().Consume(1);
                return tag;
            }

            throw new ParseException($"Expect {tag} but got {t}");
        }

        public static ReadOnlySequence<char> AsciiHexDigit(this IChunkReader<char> buffer, int count)
        {
            if (!buffer.TryPeek(count, out var r))
            {
                throw new ParseException($"Expect {count} AsciiHexDigit char but got eof");
            }
            if (r.IsSingleSegment)
            {
                foreach (var c in r.FirstSpan)
                {
                    if (!char.IsAsciiHexDigit(c))
                    {
                        throw new ParseException($"Expect AsciiHexDigit char but got {c}");
                    }
                }
                buffer.GetCurrentChunk().Consume(count);
                return r;
            }
            foreach (var item in r)
            {
                foreach (var c in r.FirstSpan)
                {
                    if (!char.IsAsciiHexDigit(c))
                    {
                        throw new ParseException($"Expect AsciiHexDigit char but got {c}");
                    }
                }
            }

            buffer.GetCurrentChunk().Consume(count);
            return r;
        }

        public static void Eof<T>(this IChunkReader<T> buffer)
        {
            if (!buffer.TryPeek(out var t))
            {
                throw new ParseException($"Expect eof but got {t}");
            }
        }
    }
}
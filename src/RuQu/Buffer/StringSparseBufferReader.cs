namespace RuQu.Buffer
{
    public class StringSparseBufferReader : ISingleChunkReader<char>
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

        public ReadOnlySpan<char> UnreadSpan => data.AsSpan(index);

        public ReadOnlyMemory<char> UnreadMemory => data.AsMemory(index);

        public int Length => data.Length;

        public int Index => index;

        public bool IsEOF => index >= data.Length;

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
}
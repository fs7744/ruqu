namespace RuQu.Buffer
{
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
}
namespace RuQu.Buffer
{
    public interface IChunkReader<T> : IDisposable
    {
        public bool IsEOF { get; }

        IChunk<T> GetCurrentChunk();
    }
}
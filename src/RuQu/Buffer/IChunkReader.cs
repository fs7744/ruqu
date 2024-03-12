namespace RuQu.Buffer
{
    public interface IChunkReader<T> : IDisposable
    {
        public bool IsEOF { get; }

        IChunk<T> GetCurrentChunk();
    }

    public interface ISingleChunkReader<T> : IDisposable, IChunk<T>, IChunkReader<T>
    {
        IChunk<T> GetCurrentChunk();
    }
}
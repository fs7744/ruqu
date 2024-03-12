namespace RuQu.Buffer
{
    public interface ISingleChunkReader<T> : IDisposable, IChunk<T>, IChunkReader<T>
    {
        IChunk<T> GetCurrentChunk();
    }
}
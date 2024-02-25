namespace RuQu.Reader
{
    public interface IReaderBuffer<T> : IDisposable
    {
        public int ConsumedCount { get; }
        public int Index { get; }
        //public ReadOnlySpan<char> Readed { get; }
        public bool IsEOF { get; }
        //public T this[int index] { get; }
        public void Consume(int count);

        public ReadOnlySpan<T> Peek(int count);

        public ValueTask<ReadOnlyMemory<T>> PeekAsync(int count, CancellationToken cancellationToken);
    }
}
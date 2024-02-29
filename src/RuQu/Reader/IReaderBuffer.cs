namespace RuQu.Reader
{
    public interface IReaderBuffer<T> : IDisposable where T : struct
    {
        public int ConsumedCount { get; }
        public int Index { get; }
        public ReadOnlySpan<T> Readed { get; }
        public ReadOnlyMemory<T> ReadedMemory { get; }
        public bool IsEOF { get; }

        public void Consume(int count);

        public bool Peek(int count, out ReadOnlySpan<T> data);

        public bool Peek(out T data);

        public bool PeekByOffset(int offset, out T data);

        public ValueTask<ReadOnlyMemory<T>?> PeekAsync(int count, CancellationToken cancellationToken = default);

        public ValueTask<T?> PeekAsync(CancellationToken cancellationToken = default);

        public ValueTask<T?> PeekByOffsetAsync(int offset, CancellationToken cancellationToken = default);

        public bool ReadNextBuffer(int count);

        public ValueTask<bool> ReadNextBufferAsync(int count, CancellationToken cancellationToken = default);
    }
}
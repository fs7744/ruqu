namespace RuQu.Reader
{
    public unsafe class ReadOnlyMemoryReadBuffer<T> : IReadBuffer<T>
    {
        internal ReadOnlyMemory<T> _buffer;
        internal int _offset;

        public ReadOnlyMemoryReadBuffer(Memory<T> memory)
        {
            _buffer = memory;
        }

        public ReadOnlyMemoryReadBuffer(ReadOnlyMemory<T> memory)
        {
            _buffer = memory;
        }

        public ReadOnlySpan<T> Remaining => _buffer.Span[_offset..];
        public bool IsFinalBlock => true;

        public void AdvanceBuffer(int bytesConsumed)
        {
        }

        public void Dispose()
        {
        }

        public void Offset(int count)
        {
            _offset = Math.Min(count + _offset, _buffer.Length);
        }

        public void ReadNextBuffer()
        {
        }

        public ValueTask<IReadBuffer<T>> ReadNextBufferAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(this as IReadBuffer<T>);
        }
    }
}
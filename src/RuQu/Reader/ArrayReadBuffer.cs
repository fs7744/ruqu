namespace RuQu.Reader
{
    public class ArrayReadBuffer<T> : IReadBuffer<T>
    {
        internal T[] _buffer;
        internal int _offset;

        public ArrayReadBuffer(T[] array)
        {
            _buffer = array;
        }

        public ReadOnlySpan<T> Remaining => _buffer.AsSpan(_offset, _buffer.Length - _offset);

        public bool IsFinalBlock => true;

        public void AdvanceBuffer(int bytesConsumed)
        {
        }

        public void Dispose()
        {
            _buffer = null!;
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
namespace RuQu.Reader
{
    public class ByteArrayReadBuffer : IReadBuffer<byte>
    {
        internal byte[] _buffer;
        internal int _offset;

        public ByteArrayReadBuffer(byte[] array)
        {
            _buffer = array;
        }

        public ReadOnlySpan<byte> Remaining => _buffer.AsSpan(_offset, _buffer.Length - _offset);

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

        public ValueTask<IReadBuffer<byte>> ReadNextBufferAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(this as IReadBuffer<byte>);
        }
    }
}
namespace RuQu.Reader
{
    public class CharArrayReadBuffer : IReadBuffer<char>
    {
        internal char[] _buffer;
        internal int _offset;

        public CharArrayReadBuffer(char[] array)
        {
            _buffer = array;
        }

        public ReadOnlySpan<char> Remaining => _buffer.AsSpan(_offset, _buffer.Length - _offset);

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

        public ValueTask<IReadBuffer<char>> ReadNextBufferAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(this as IReadBuffer<char>);
        }
    }
}
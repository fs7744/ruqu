﻿namespace RuQu.Reader
{
    public class StringReadBuffer : IReadBuffer<char>
    {
        internal string _buffer;
        internal int _offset;

        public StringReadBuffer(string str)
        {
            _buffer = str;
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
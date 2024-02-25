using System.Runtime.CompilerServices;

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

        public ReadOnlySpan<T> Remaining
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Span[_offset..];
        }

        public int RemainingCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length - _offset;
        }

        public bool IsFinalBlock
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceBuffer(int bytesConsumed)
        {
        }

        public void Dispose()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(int count)
        {
            _offset = Math.Min(count + _offset, _buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadNextBuffer()
        {
        }

        public ValueTask<IReadBuffer<T>> ReadNextBufferAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(this as IReadBuffer<T>);
        }
    }
}
using System.Runtime.CompilerServices;

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

        public ReadOnlySpan<T> Remaining
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan(_offset, _buffer.Length - _offset);
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
            _buffer = null!;
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
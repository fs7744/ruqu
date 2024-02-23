using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RuQu.Reader
{
    public unsafe class ReadOnlySpanReadBuffer<T> : IReadBuffer<T>
    {
        internal void* _buffer;
        internal int _offset;
        internal int _length;

        public ReadOnlySpanReadBuffer(Span<T> span)
        {
            _buffer = Unsafe.AsPointer(ref span.GetPinnableReference());
            _length = span.Length;
        }

        public ReadOnlySpanReadBuffer(ReadOnlySpan<T> span)
        {
            _buffer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            _length = span.Length;
        }

        public ReadOnlySpan<T> Remaining => new ReadOnlySpan<T>(_buffer, _length)[_offset.._length];
        public bool IsFinalBlock => true;

        public void AdvanceBuffer(int bytesConsumed)
        {
        }

        public void Dispose()
        {
        }

        public void Offset(int count)
        {
            _offset = Math.Min(count + _offset, _length);
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
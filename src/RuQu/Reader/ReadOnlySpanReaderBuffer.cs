using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RuQu.Reader
{
    public unsafe struct ReadOnlySpanReaderBuffer<T> : IReaderBuffer<T> where T : struct
    {
        internal void* _buffer;
        internal int _offset;
        internal int _length;
        internal int _consumedCount;

        public ReadOnlySpanReaderBuffer(Span<T> span)
        {
            _buffer = Unsafe.AsPointer(ref span.GetPinnableReference());
            _length = span.Length;
        }

        public ReadOnlySpanReaderBuffer(ReadOnlySpan<T> span)
        {
            _buffer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            _length = span.Length;
        }

        public ReadOnlySpan<T> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ReadOnlySpan<T>(_buffer, _length)[_offset.._length];
        }

        public bool IsEOF
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _offset == _length;
        }

        public int ConsumedCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _consumedCount;
        }

        public int Index
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _offset;
        }

        public void Consume(int count)
        {
            _offset += count;
            _consumedCount += count;
        }

        public void Dispose()
        {
        }

        public bool Peek(int count, out ReadOnlySpan<T> data)
        {
            if (_offset + count > _length)
            {
                data = default;
                return false;
            }
            data = new ReadOnlySpan<T>(_buffer, _length).Slice(_offset, count);
            return true;
        }

        public bool Peek(out T data)
        {
            if (_offset >= _length)
            {
                data = default;
                return false;
            }
            data = new ReadOnlySpan<T>(_buffer, _length)[_offset];
            return true;
        }

        public ValueTask<ReadOnlyMemory<T>?> PeekAsync(int count, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask<T?> PeekAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public bool ReadNextBuffer(int count) => false;

        public ValueTask<bool> ReadNextBufferAsync(int count, CancellationToken cancellationToken = default) => ValueTask.FromResult<bool>(false);
    }
}
using System.Runtime.CompilerServices;

namespace RuQu.Reader
{
    public unsafe class ReadOnlyMemoryReaderBuffer<T> : IFixedReaderBuffer<T> where T : struct
    {
        internal ReadOnlyMemory<T> _buffer;
        internal int _offset;
        internal int _consumedCount;

        public ReadOnlyMemoryReaderBuffer(Memory<T> memory)
        {
            _buffer = memory;
        }

        public ReadOnlyMemoryReaderBuffer(ReadOnlyMemory<T> memory)
        {
            _buffer = memory;
        }

        public ReadOnlySpan<T> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Span[_offset..];
        }

        public bool IsEOF
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _offset == _buffer.Length;
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
            if (_offset + count > _buffer.Length || count <= 0)
            {
                data = default;
                return false;
            }
            data = _buffer.Span.Slice(_offset, count);
            return true;
        }

        public bool Peek(out T data)
        {
            if (_offset >= _buffer.Length)
            {
                data = default;
                return false;
            }
            data = _buffer.Span[_offset];
            return true;
        }

        public bool PeekByOffset(int offset, out T data)
        {
            var o = _offset + offset;
            if (o >= _buffer.Length)
            {
                data = default;
                return false;
            }
            data = _buffer.Span[o];
            return true;
        }

        public ValueTask<ReadOnlyMemory<T>?> PeekAsync(int count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset + count > _buffer.Length || count <= 0)
            {
                return ValueTask.FromResult<ReadOnlyMemory<T>?>(null);
            }
            return ValueTask.FromResult<ReadOnlyMemory<T>?>(_buffer.Slice(_offset, count));
        }

        public ValueTask<T?> PeekAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset >= _buffer.Length)
            {
                return ValueTask.FromResult<T?>(null);
            }
            return ValueTask.FromResult<T?>(_buffer.Span[_offset]);
        }

        public ValueTask<T?> PeekByOffsetAsync(int offset, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var o = _offset + offset;
            if (o >= _buffer.Length)
            {
                return ValueTask.FromResult<T?>(null);
            }
            return ValueTask.FromResult<T?>(_buffer.Span[o]);
        }

        public bool ReadNextBuffer(int count) => false;

        public ValueTask<bool> ReadNextBufferAsync(int count, CancellationToken cancellationToken = default) => ValueTask.FromResult<bool>(false);
    }
}
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RuQu.Reader
{
    public unsafe class ReadOnlySpanReaderBuffer<T> : MemoryManager<T>, IFixedReaderBuffer<T> where T : struct
    {
        internal T* _buffer;
        internal int _offset;
        internal int _length;
        internal int _consumedCount;

        public ReadOnlySpanReaderBuffer(Span<T> span)
        {
            fixed (T* ptr = &span.GetPinnableReference())
            {
                _buffer = ptr;
                _length = span.Length;
            }
        }

        public ReadOnlySpanReaderBuffer(ReadOnlySpan<T> span)
        {
            fixed (T* ptr = &MemoryMarshal.GetReference(span))
            {
                _buffer = ptr;
                _length = span.Length;
            }
        }

        public ReadOnlySpan<T> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ReadOnlySpan<T>(Unsafe.Add<T>(_buffer, _offset), _length);
        }

        public ReadOnlyMemory<T> ReadedMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Memory.Slice(_offset);
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
            if (_offset + count > _length || count <= 0)
            {
                data = default;
                return false;
            }
            data = new ReadOnlySpan<T>(Unsafe.Add<T>(_buffer, _offset), count);
            return true;
        }

        public bool Peek(out T data)
        {
            if (_offset >= _length)
            {
                data = default;
                return false;
            }
            data = Unsafe.As<byte, T>(ref *(byte*)Unsafe.Add<T>(_buffer, _offset));
            return true;
        }

        public bool PeekByOffset(int offset, out T data)
        {
            var o = _offset + offset;
            if (o >= _length)
            {
                data = default;
                return false;
            }
            data = Unsafe.As<byte, T>(ref *(byte*)Unsafe.Add<T>(_buffer, o));
            return true;
        }

        public ValueTask<ReadOnlyMemory<T>?> PeekAsync(int count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset + count > _length || count <= 0)
            {
                return ValueTask.FromResult<ReadOnlyMemory<T>?>(null);
            }
            return ValueTask.FromResult<ReadOnlyMemory<T>?>(ReadedMemory.Slice(0, count));
        }

        public ValueTask<T?> PeekAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Peek(out var data))
            {
                return ValueTask.FromResult<T?>(data);
            }
            return ValueTask.FromResult<T?>(null);
        }

        public ValueTask<T?> PeekByOffsetAsync(int offset, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (PeekByOffset(offset, out var data))
            {
                return ValueTask.FromResult<T?>(data);
            }
            return ValueTask.FromResult<T?>(null);
        }

        public bool ReadNextBuffer(int count) => false;

        public ValueTask<bool> ReadNextBufferAsync(int count, CancellationToken cancellationToken = default) => ValueTask.FromResult<bool>(false);

        protected override void Dispose(bool disposing)
        {
        }

        public override Span<T> GetSpan()
        {
            return new Span<T>(_buffer, _length);
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= _length)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));
            return new MemoryHandle(_buffer + elementIndex);
        }

        public override void Unpin()
        {
        }
    }
}
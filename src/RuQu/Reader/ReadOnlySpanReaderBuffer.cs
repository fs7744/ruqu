using RuQu.Writer;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RuQu.Reader
{
    public unsafe class ReadOnlySpanReaderBuffer<T> : IFixedReaderBuffer<T> where T : struct
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
            get => new ReadOnlySpan<T>(Unsafe.Add<T>(_buffer, _offset), _length);
        }

        public ReadOnlyMemory<T> ReadedMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new UnmanagedMemoryManager<T>((IntPtr)Unsafe.Add<T>(_buffer, _offset), _length).Memory;
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
            return ValueTask.FromResult<ReadOnlyMemory<T>?>(new UnmanagedMemoryManager<T>((IntPtr)Unsafe.Add<T>(_buffer, _offset), count).Memory);
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
    }
}
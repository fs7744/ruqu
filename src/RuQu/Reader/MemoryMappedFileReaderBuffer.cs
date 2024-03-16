using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace RuQu.Reader
{
    public unsafe class MemoryMappedFileReaderBuffer : MemoryManager<byte>, IFixedReaderBuffer<byte>
    {
        private readonly byte* _pointer;
        private readonly int _fileLength;
        private readonly MemoryMappedFile _mapping;
        private readonly MemoryMappedViewAccessor _viewAccessor;
        internal int _offset;
        internal int _consumedCount;

        public MemoryMappedFileReaderBuffer(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _fileLength = (int)fs.Length;
            }
            _mapping = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _viewAccessor = _mapping.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            _viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _pointer);
        }

        public ReadOnlySpan<byte> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ReadOnlySpan<byte>(_pointer + _offset, _fileLength - _offset);
        }

        public ReadOnlyMemory<byte> ReadedMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Memory.Slice(_offset);
        }

        public bool IsEOF
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _offset == _fileLength;
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
            _mapping.Dispose();
            _viewAccessor.Dispose();
        }

        public bool Peek(int count, out ReadOnlySpan<byte> data)
        {
            if (_offset + count > _fileLength || count <= 0)
            {
                data = default;
                return false;
            }
            data = new ReadOnlySpan<byte>(_pointer + _offset, count);
            return true;
        }

        public bool Peek(out byte data)
        {
            if (_offset >= _fileLength)
            {
                data = default;
                return false;
            }
            data = *(_pointer + _offset);
            return true;
        }

        public bool PeekByOffset(int offset, out byte data)
        {
            var o = _offset + offset;
            if (o >= _fileLength)
            {
                data = default;
                return false;
            }
            data = *(_pointer + o);
            return true;
        }

        public ValueTask<ReadOnlyMemory<byte>?> PeekAsync(int count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset + count > _fileLength || count <= 0)
            {
                return ValueTask.FromResult<ReadOnlyMemory<byte>?>(null);
            }
            return ValueTask.FromResult<ReadOnlyMemory<byte>?>(ReadedMemory.Slice(0, count));
        }

        public ValueTask<byte?> PeekAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Peek(out var data))
            {
                return ValueTask.FromResult<byte?>(data);
            }
            return ValueTask.FromResult<byte?>(null);
        }

        public ValueTask<byte?> PeekByOffsetAsync(int offset, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (PeekByOffset(offset, out var data))
            {
                return ValueTask.FromResult<byte?>(data);
            }
            return ValueTask.FromResult<byte?>(null);
        }

        public bool ReadNextBuffer(int count) => false;

        public ValueTask<bool> ReadNextBufferAsync(int count, CancellationToken cancellationToken = default) => ValueTask.FromResult<bool>(false);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Dispose();
            }
        }

        public override Span<byte> GetSpan()
        {
            return new Span<byte>(_pointer, _fileLength);
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= _fileLength)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));
            return new MemoryHandle(_pointer + elementIndex);
        }

        public override void Unpin()
        {
        }
    }
}
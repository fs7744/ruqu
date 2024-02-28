using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace RuQu.Reader
{
    public unsafe class MemoryMappedFileReaderBuffer : IFixedReaderBuffer<byte>
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
            get => new ReadOnlySpan<byte>(_pointer, _fileLength)[_offset.._fileLength];
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
            throw new NotSupportedException();
        }

        public ValueTask<byte?> PeekAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public ValueTask<byte?> PeekByOffsetAsync(int offset, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool ReadNextBuffer(int count) => false;

        public ValueTask<bool> ReadNextBufferAsync(int count, CancellationToken cancellationToken = default) => ValueTask.FromResult<bool>(false);
    }
}

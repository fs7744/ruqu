using System.Buffers;
using System.Runtime.CompilerServices;

namespace RuQu.Reader
{
    public class TextReaderBuffer : IReaderBuffer<char>
    {
        internal char[] _buffer;
        internal int _offset;
        internal int _count;
        internal int _maxCount;
        internal int _consumedCount;
        private TextReader _reader;
        private bool _isFinalBlock;
        private bool _isReaded;

        public ReadOnlySpan<char> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_isReaded)
                {
                    ReadNextBuffer(1);
                    _isReaded = true;
                }
                return _buffer.AsSpan(_offset, _count - _offset);
            }
        }

        public ReadOnlyMemory<char> ReadedMemory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!_isReaded)
                {
                    ReadNextBuffer(1);
                    _isReaded = true;
                }
                return _buffer.AsMemory(_offset, _count - _offset);
            }
        }

        public bool IsEOF
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isFinalBlock && _offset == _count;
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

        public TextReaderBuffer(TextReader reader, int initialBufferSize)
        {
            if (initialBufferSize <= 0)
            {
                initialBufferSize = 256;
            }
            _buffer = ArrayPool<char>.Shared.Rent(initialBufferSize);
            _consumedCount = _count = _offset = 0;
            _reader = reader;
        }

        public void Consume(int count)
        {
            _offset += count;
            _consumedCount += count;
        }

        public void AdvanceBuffer(int count)
        {
            var remaining = _buffer.Length - _count + _offset;
            if (remaining <= (_buffer.Length / 2) && _buffer.Length != int.MaxValue)
            {
                // We have less than half the buffer available, double the buffer size.
                char[] oldBuffer = _buffer;
                int oldMaxCount = _maxCount;
                var newSize = (_buffer.Length < (int.MaxValue / 2)) ? _buffer.Length * 2 : int.MaxValue;
                while (newSize < count)
                {
                    newSize *= (newSize < (int.MaxValue / 2)) ? newSize * 2 : int.MaxValue;
                }
                char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
                // Copy the unprocessed data to the new buffer while shifting the processed bytes.
                Buffer.BlockCopy(oldBuffer, _offset, newBuffer, 0, _count - _offset);
                _buffer = newBuffer;
                // Clear and return the old buffer
                new Span<char>(oldBuffer, 0, oldMaxCount).Clear();
                ArrayPool<char>.Shared.Return(oldBuffer);
                _maxCount = _count;
                _count -= _offset;
                _offset = 0;
            }
            else if (_offset != 0)
            {
                _count -= _offset;
                // Shift the processed bytes to the beginning of buffer to make more room.
                Buffer.BlockCopy(_buffer, _offset, _buffer, 0, _count);
                _offset = 0;
            }
        }

        public void Dispose()
        {
            if (_buffer != null)
            {
                new Span<char>(_buffer, 0, _maxCount).Clear();
                char[] toReturn = _buffer;
                ArrayPool<char>.Shared.Return(toReturn);
                _buffer = null!;
            }
        }

        public bool Peek(int count, out ReadOnlySpan<char> data)
        {
            if (!_isReaded)
            {
                ReadNextBuffer(count);
                _isReaded = true;
            }
            if (!_isFinalBlock && count + _offset > _count)
            {
                ReadNextBuffer(count);
            }
            if (_offset + count > _count || count <= 0)
            {
                data = default;
                return false;
            }
            data = _buffer.AsSpan(_offset, count);
            return true;
        }

        public bool Peek(out char data)
        {
            if (!_isReaded)
            {
                ReadNextBuffer(1);
                _isReaded = true;
            }
            if (!_isFinalBlock && 1 + _offset > _count)
            {
                ReadNextBuffer(1);
            }
            if (_offset >= _count)
            {
                data = default;
                return false;
            }
            data = _buffer[_offset];
            return true;
        }

        public bool PeekByOffset(int offset, out char data)
        {
            var o = offset + _offset;
            if (!_isReaded)
            {
                ReadNextBuffer(o);
                _isReaded = true;
            }
            if (!_isFinalBlock && o > _count)
            {
                ReadNextBuffer(o);
            }
            if (o >= _count)
            {
                data = default;
                return false;
            }
            data = _buffer[o];
            return true;
        }

        public bool ReadNextBuffer(int count)
        {
            if (!_isFinalBlock)
            {
                AdvanceBuffer(count);
                do
                {
                    int readCount = _reader.Read(_buffer.AsSpan(_count));
                    if (readCount == 0)
                    {
                        _isFinalBlock = true;
                        break;
                    }

                    _count += readCount;
                }
                while (_count < _buffer.Length);

                if (_count > _maxCount)
                {
                    _maxCount = _count;
                }
                return true;
            }
            return false;
        }

        public async ValueTask<bool> ReadNextBufferAsync(int count, CancellationToken cancellationToken = default)
        {
            if (!_isFinalBlock)
            {
                AdvanceBuffer(count);
                do
                {
                    int readCount = await _reader.ReadAsync(_buffer.AsMemory(_count), cancellationToken);
                    if (readCount == 0)
                    {
                        _isFinalBlock = true;
                        break;
                    }

                    _count += readCount;
                }
                while (_count < _buffer.Length);

                if (_count > _maxCount)
                {
                    _maxCount = _count;
                }
                return true;
            }
            return false;
        }

        public async ValueTask<ReadOnlyMemory<char>?> PeekAsync(int count, CancellationToken cancellationToken = default)
        {
            if (!_isReaded)
            {
                await ReadNextBufferAsync(count, cancellationToken);
                _isReaded = true;
            }
            var o = count + _offset;
            if (!_isFinalBlock &&  o > _count)
            {
                await ReadNextBufferAsync(count, cancellationToken);
            }
            if (o > _count || count <= 0)
            {
                return null;
            }
            return _buffer.AsMemory(_offset, count);
        }

        public async ValueTask<char?> PeekAsync(CancellationToken cancellationToken = default)
        {
            if (!_isReaded)
            {
                await ReadNextBufferAsync(1, cancellationToken);
                _isReaded = true;
            }
            if (!_isFinalBlock && 1 + _offset > _count)
            {
                await ReadNextBufferAsync(1, cancellationToken);
            }
            if (_offset >= _count)
            {
                return null;
            }
            return _buffer[_offset];
        }

        public async ValueTask<char?> PeekByOffsetAsync(int offset, CancellationToken cancellationToken = default)
        {
            var o = offset + _offset;
            if (!_isReaded)
            {
                await ReadNextBufferAsync(o, cancellationToken);
                _isReaded = true;
            }
            if (!_isFinalBlock && o > _count)
            {
                await ReadNextBufferAsync(o, cancellationToken);
            }
            if (o >= _count)
            {
                return null;
            }
            return _buffer[o];
        }
    }
}
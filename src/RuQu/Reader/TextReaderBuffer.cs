using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RuQu.Reader
{
    public class StringReaderBuffer : IReaderBuffer<char>
    {
        internal string _buffer;
        internal int _offset;
        internal int _consumedCount;

        public StringReaderBuffer(string content)
        {
            _buffer = content;
        }

        public char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer[_offset];
        }

        public ReadOnlySpan<char> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan(_offset);
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
            _buffer = null;
        }

        public ReadOnlySpan<char> Peek(int count)
        {
            return _buffer.AsSpan(_offset, Math.Min(count, _buffer.Length - _offset));
        }

        public ValueTask<ReadOnlyMemory<char>> PeekAsync(int count, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(_buffer.AsMemory(_offset, Math.Min(count, _buffer.Length - _offset)));
        }
    }

    public class TextReaderBuffer : IReaderBuffer<char>
    {
        internal char[] _buffer;
        internal int _offset;
        internal int _count;
        internal int _maxCount;
        internal int _consumedCount;
        private TextReader _reader;
        private bool _isFinalBlock;

        public char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer[_offset];
        }

        public ReadOnlySpan<char> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan(_offset, _count - _offset);
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

        public async ValueTask<IReaderBuffer<char>> ConsumeAsync(int count, CancellationToken cancellationToken)
        {
            var buffer = this;
            cancellationToken.ThrowIfCancellationRequested();
            AdvanceBuffer(count);
            if (!_isFinalBlock && _count < _buffer.Length)
            {
                do
                {
                    int readCount = await _reader.ReadAsync(buffer._buffer.AsMemory(buffer._count), cancellationToken).ConfigureAwait(false);
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
            }
            return buffer;
        }

        public void AdvanceBuffer(int count)
        {
            if (!_isFinalBlock)
            {
                // Check if we need to shift or expand the buffer because there wasn't enough data to complete deserialization.
                if ((uint)_count > ((uint)_buffer.Length / 2) && _buffer.Length != int.MaxValue)
                {
                    // We have less than half the buffer available, double the buffer size.
                    char[] oldBuffer = _buffer;
                    int oldMaxCount = _maxCount;
                    char[] newBuffer = ArrayPool<char>.Shared.Rent((_buffer.Length < (int.MaxValue / 2)) ? _buffer.Length * 2 : int.MaxValue);

                    // Copy the unprocessed data to the new buffer while shifting the processed bytes.
                    Buffer.BlockCopy(oldBuffer, _offset, newBuffer, 0, _count);
                    _buffer = newBuffer;
                    _maxCount = _count;

                    // Clear and return the old buffer
                    new Span<char>(oldBuffer, 0, oldMaxCount).Clear();
                    ArrayPool<char>.Shared.Return(oldBuffer);
                    _offset = 0;
                }
            }
        }

        public void Dispose()
        {
            _reader = null;

            if (_buffer != null)
            {
                new Span<char>(_buffer, 0, _maxCount).Clear();
                char[] toReturn = _buffer;
                ArrayPool<char>.Shared.Return(toReturn);
                _buffer = null!;
            }
        }

        public ReadOnlySpan<char> Peek(int count)
        {
            if (!_isFinalBlock && _count < _buffer.Length)
            {
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
            }
            return _buffer.AsSpan(_offset, Math.Min(count, _count - _offset));
        }

        public async ValueTask<ReadOnlyMemory<char>> PeekAsync(int count, CancellationToken cancellationToken)
        {
            if (!_isFinalBlock && _count < _buffer.Length)
            {
                do
                {
                    int readCount = await _reader.ReadAsync(_buffer.AsMemory(_count), cancellationToken).ConfigureAwait(false);
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
            }
            return _buffer.AsMemory(_offset, Math.Min(count, _count - _offset));
        }
    }
}
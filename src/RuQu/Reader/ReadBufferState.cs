using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RuQu.Reader
{
    [StructLayout(LayoutKind.Auto)]
    public struct ReadBuffer : IDisposable
    {
        internal byte[] _buffer;
        internal byte _offset;
        internal int _count;
        internal int _maxCount;
        private bool _isFinalBlock;

        public ReadBuffer(int initialBufferSize)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialBufferSize);
            _maxCount = _count = _offset = 0;
            _isFinalBlock = false;
        }

        public readonly bool IsFinalBlock => _isFinalBlock;

        public readonly ReadOnlySpan<byte> Bytes => _buffer.AsSpan(_offset, _count);

        public async ValueTask<ReadBuffer> ReadFromStreamAsync(
            Stream stream,
            CancellationToken cancellationToken,
            bool fillBuffer = true)
        {
            // Since mutable structs don't work well with async state machines,
            // make all updates on a copy which is returned once complete.
            ReadBuffer bufferState = this;

            do
            {
                int bytesRead = await stream.ReadAsync(bufferState._buffer.AsMemory(bufferState._count), cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    bufferState._isFinalBlock = true;
                    break;
                }

                bufferState._count += bytesRead;
            }
            while (fillBuffer && bufferState._count < bufferState._buffer.Length);

            if (_count > _maxCount)
            {
                _maxCount = _count;
            }
            return bufferState;
        }

        public void ReadFromStream(Stream stream)
        {
            do
            {
                int bytesRead = stream.Read(_buffer.AsSpan(_count));
                if (bytesRead == 0)
                {
                    _isFinalBlock = true;
                    break;
                }

                _count += bytesRead;
            }
            while (_count < _buffer.Length);

            if (_count > _maxCount)
            {
                _maxCount = _count;
            }
        }

        public void AdvanceBuffer(int bytesConsumed)
        {
            Debug.Assert(bytesConsumed <= _count);
            Debug.Assert(!_isFinalBlock || _count == bytesConsumed, "The reader should have thrown if we have remaining bytes.");

            _count -= bytesConsumed;

            if (!_isFinalBlock)
            {
                // Check if we need to shift or expand the buffer because there wasn't enough data to complete deserialization.
                if ((uint)_count > ((uint)_buffer.Length / 2))
                {
                    // We have less than half the buffer available, double the buffer size.
                    byte[] oldBuffer = _buffer;
                    int oldMaxCount = _maxCount;
                    byte[] newBuffer = ArrayPool<byte>.Shared.Rent((_buffer.Length < (int.MaxValue / 2)) ? _buffer.Length * 2 : int.MaxValue);

                    // Copy the unprocessed data to the new buffer while shifting the processed bytes.
                    Buffer.BlockCopy(oldBuffer, _offset + bytesConsumed, newBuffer, 0, _count);
                    _buffer = newBuffer;
                    _maxCount = _count;

                    // Clear and return the old buffer
                    new Span<byte>(oldBuffer, 0, oldMaxCount).Clear();
                    ArrayPool<byte>.Shared.Return(oldBuffer);
                }
                else if (_count != 0)
                {
                    // Shift the processed bytes to the beginning of buffer to make more room.
                    Buffer.BlockCopy(_buffer, _offset + bytesConsumed, _buffer, 0, _count);
                }
            }

            _offset = 0;
        }

        public void Dispose()
        {
            // Clear only what we used and return the buffer to the pool
            new Span<byte>(_buffer, 0, _maxCount).Clear();

            byte[] toReturn = _buffer;
            _buffer = null!;

            ArrayPool<byte>.Shared.Return(toReturn);
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public struct ReadCharBuffer : IDisposable
    {
        internal char[] _buffer;
        internal byte _offset;
        internal int _count;
        internal int _maxCount;
        private bool _isFinalBlock;

        public ReadCharBuffer(int initialBufferSize)
        {
            _buffer = ArrayPool<char>.Shared.Rent(initialBufferSize);
            _maxCount = _count = _offset = 0;
            _isFinalBlock = false;
        }

        public readonly bool IsFinalBlock => _isFinalBlock;

        public readonly ReadOnlySpan<char> Chars => _buffer.AsSpan(_offset, _count);

        public async ValueTask<ReadCharBuffer> ReadFromStreamAsync(
            TextReader reader,
            CancellationToken cancellationToken,
            bool fillBuffer = true)
        {
            // Since mutable structs don't work well with async state machines,
            // make all updates on a copy which is returned once complete.
            ReadCharBuffer bufferState = this;

            do
            {
                int bytesRead = await reader.ReadAsync(bufferState._buffer.AsMemory(bufferState._count), cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    bufferState._isFinalBlock = true;
                    break;
                }

                bufferState._count += bytesRead;
            }
            while (fillBuffer && bufferState._count < bufferState._buffer.Length);

            if (_count > _maxCount)
            {
                _maxCount = _count;
            }
            return bufferState;
        }

        public void ReadFromStream(TextReader reader)
        {
            do
            {
                int bytesRead = reader.Read(_buffer.AsSpan(_count));
                if (bytesRead == 0)
                {
                    _isFinalBlock = true;
                    break;
                }

                _count += bytesRead;
            }
            while (_count < _buffer.Length);

            if (_count > _maxCount)
            {
                _maxCount = _count;
            }
        }

        public void AdvanceBuffer(int bytesConsumed)
        {
            Debug.Assert(bytesConsumed <= _count);
            Debug.Assert(!_isFinalBlock || _count == bytesConsumed, "The reader should have thrown if we have remaining bytes.");

            _count -= bytesConsumed;

            if (!_isFinalBlock)
            {
                // Check if we need to shift or expand the buffer because there wasn't enough data to complete deserialization.
                if ((uint)_count > ((uint)_buffer.Length / 2))
                {
                    // We have less than half the buffer available, double the buffer size.
                    char[] oldBuffer = _buffer;
                    int oldMaxCount = _maxCount;
                    char[] newBuffer = ArrayPool<char>.Shared.Rent((_buffer.Length < (int.MaxValue / 2)) ? _buffer.Length * 2 : int.MaxValue);

                    // Copy the unprocessed data to the new buffer while shifting the processed bytes.
                    Buffer.BlockCopy(oldBuffer, _offset + bytesConsumed, newBuffer, 0, _count);
                    _buffer = newBuffer;
                    _maxCount = _count;

                    // Clear and return the old buffer
                    new Span<char>(oldBuffer, 0, oldMaxCount).Clear();
                    ArrayPool<char>.Shared.Return(oldBuffer);
                }
                else if (_count != 0)
                {
                    // Shift the processed bytes to the beginning of buffer to make more room.
                    Buffer.BlockCopy(_buffer, _offset + bytesConsumed, _buffer, 0, _count);
                }
            }

            _offset = 0;
        }

        public void Dispose()
        {
            // Clear only what we used and return the buffer to the pool
            new Span<char>(_buffer, 0, _maxCount).Clear();

            char[] toReturn = _buffer;
            _buffer = null!;

            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    public static class ReadBufferStateExtensions
    {
        public static ReadOnlySpan<byte> Utf8Bom => [0xEF, 0xBB, 0xBF];

        public static void IngoreUtf8Bom(this ref ReadBuffer buffer)
        {
            // Handle the UTF-8 BOM if present
            Debug.Assert(buffer._buffer.Length >= Utf8Bom.Length);
            if (buffer._buffer.AsSpan(0, buffer._count).StartsWith(Utf8Bom))
            {
                buffer._offset = (byte)Utf8Bom.Length;
                buffer._count -= Utf8Bom.Length;
            }
        }
    }
}
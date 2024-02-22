using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace RuQu.Reader
{
    public interface IReadBuffer<T> : IDisposable
    {
        public ReadOnlySpan<T> Remaining { get; }
        public bool IsFinalBlock { get; }

        public void Offset(int count);

        public void AdvanceBuffer(int bytesConsumed);

        public void ReadNextBuffer();

        public ValueTask<IReadBuffer<T>> ReadNextBufferAsync(CancellationToken cancellationToken);
    }


    [StructLayout(LayoutKind.Auto)]
    public struct ByteReadBuffer : IReadBuffer<byte>
    {
        internal byte[] _buffer;
        internal int _offset;
        internal int _count;
        internal int _maxCount;
        private bool _isFinalBlock;
        private Stream _stream;

        public readonly ReadOnlySpan<byte> Remaining => _buffer.AsSpan(_offset, _count - _offset);

        public readonly bool IsFinalBlock => _isFinalBlock;

        public ByteReadBuffer(Stream stream ,int initialBufferSize)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialBufferSize);
            _maxCount = _count = _offset = 0;
            _isFinalBlock = false;
            _stream = stream;
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
            _stream= null!;
        }

        public void Offset(int count)
        {
            _offset = Math.Min(count + _offset, _count);
        }

        public void ReadNextBuffer()
        {
            do
            {
                int bytesRead = _stream.Read(_buffer.AsSpan(_count));
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

        public async ValueTask<IReadBuffer<byte>> ReadNextBufferAsync(CancellationToken cancellationToken)
        {
            // Since mutable structs don't work well with async state machines,
            // make all updates on a copy which is returned once complete.
            ByteReadBuffer bufferState = this;

            do
            {
                int bytesRead = await _stream.ReadAsync(bufferState._buffer.AsMemory(bufferState._count), cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    bufferState._isFinalBlock = true;
                    break;
                }

                bufferState._count += bytesRead;
            }
            while (bufferState._count < bufferState._buffer.Length);

            if (_count > _maxCount)
            {
                _maxCount = _count;
            }
            return bufferState;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public class CharReadBuffer : IReadBuffer<char>
    {
        internal char[] _buffer;
        internal int _offset;
        internal int _count;
        internal int _maxCount;
        private bool _isFinalBlock;
        private TextReader _stream;

        public ReadOnlySpan<char> Remaining => _buffer.AsSpan(_offset, _count - _offset);

        public bool IsFinalBlock => _isFinalBlock;

        public CharReadBuffer(TextReader stream, int initialBufferSize)
        {
            _buffer = ArrayPool<char>.Shared.Rent(initialBufferSize);
            _maxCount = _count = _offset = 0;
            _isFinalBlock = false;
            _stream = stream;
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
            _stream = null!;
        }

        public void Offset(int count)
        {
            _offset = Math.Min(count + _offset, _count);
        }

        public void ReadNextBuffer()
        {
            do
            {
                int bytesRead = _stream.Read(_buffer.AsSpan(_count));
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

        public async ValueTask<IReadBuffer<char>> ReadNextBufferAsync(CancellationToken cancellationToken)
        {
            // Since mutable structs don't work well with async state machines,
            // make all updates on a copy which is returned once complete.
            CharReadBuffer bufferState = this;

            do
            {
                int bytesRead = await _stream.ReadAsync(bufferState._buffer.AsMemory(bufferState._count), cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    bufferState._isFinalBlock = true;
                    break;
                }

                bufferState._count += bytesRead;
            }
            while (bufferState._count < bufferState._buffer.Length);

            if (_count > _maxCount)
            {
                _maxCount = _count;
            }
            return bufferState;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public class StringReadBuffer : IReadBuffer<char>
    {
        internal string _buffer;
        internal int _offset;

        public StringReadBuffer(string str)
        {
            _buffer = str;
        }

        public ReadOnlySpan<char> Remaining => _buffer.AsSpan(_offset, _buffer.Length - _offset);

        public bool IsFinalBlock => true;

        public void AdvanceBuffer(int bytesConsumed)
        {
        }

        public void Dispose()
        {
            _buffer = null!;
        }

        public void Offset(int count)
        {
            _offset = Math.Min(count + _offset, _buffer.Length);
        }

        public void ReadNextBuffer()
        {
        }

        public ValueTask<IReadBuffer<char>> ReadNextBufferAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(this as IReadBuffer<char>);
        }
    }


    public static class ReadBufferStateExtensions
    {
        public static ReadOnlySpan<byte> Utf8Bom => [0xEF, 0xBB, 0xBF];

        public static void IngoreUtf8Bom(this ref ByteReadBuffer buffer)
        {
            var remaining = buffer.Remaining;
            // Handle the UTF-8 BOM if present
            Debug.Assert(remaining.Length >= Utf8Bom.Length);
            if (remaining.StartsWith(Utf8Bom))
            {
                buffer.Offset(Utf8Bom.Length);
            }
        }

        public static int ReadLine(this IReadBuffer<char> buffer, out ReadOnlySpan<char> span)
        {
            ReadOnlySpan<char> remaining = buffer.Remaining;
            if(!remaining.IsEmpty) 
            {
                int foundLineLength = remaining.IndexOfAny('\r', '\n');
                if (foundLineLength >= 0)
                {
                    span = remaining[0..foundLineLength];
                    char ch = remaining[foundLineLength];
                    var pos = foundLineLength + 1;
                    if (ch == '\r')
                    {
                        if ((uint)pos < (uint)remaining.Length && remaining[pos] == '\n')
                        {
                            pos++;
                        }
                    }
                    buffer.Offset(pos);
                    return pos;
                }
            }
            span = default;
            return 0;
        }
    }
}
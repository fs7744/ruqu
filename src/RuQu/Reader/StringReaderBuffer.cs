﻿using System.Runtime.CompilerServices;

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
        }

        public bool Peek(int count, out ReadOnlySpan<char> data)
        {
            if (_offset + count > _buffer.Length)
            {
                data = default;
                return false;
            }
            data = _buffer.AsSpan(_offset, count);
            return true;
        }

        public bool Peek(out char data)
        {
            if (_offset >= _buffer.Length)
            {
                data = default;
                return false;
            }
            data = _buffer[_offset];
            return true;
        }

        public ValueTask<ReadOnlyMemory<char>?> PeekAsync(int count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset + count > _buffer.Length)
            {
                return ValueTask.FromResult<ReadOnlyMemory<char>?>(null);
            }
            return ValueTask.FromResult<ReadOnlyMemory<char>?>(_buffer.AsMemory(_offset, count));
        }

        public ValueTask<char?> PeekAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset >= _buffer.Length)
            {
                return ValueTask.FromResult<char?>(null);
            }
            return ValueTask.FromResult<char?>(_buffer[_offset]);
        }
    }
}
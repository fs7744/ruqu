﻿using System.Runtime.CompilerServices;

namespace RuQu.Reader
{
    public class ArrayReaderBuffer<T> : IReaderBuffer<T> where T : struct
    {
        internal T[] _buffer;
        internal int _offset;
        internal int _consumedCount;

        public ArrayReaderBuffer(T[] array)
        {
            _buffer = array;
        }

        public ReadOnlySpan<T> Readed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan(_offset, _buffer.Length - _offset);
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
            if (_offset + count > _buffer.Length)
            {
                data = default;
                return false;
            }
            data = _buffer.AsSpan(_offset, count);
            return true;
        }

        public bool Peek(out T data)
        {
            if (_offset >= _buffer.Length)
            {
                data = default;
                return false;
            }
            data = _buffer[_offset];
            return true;
        }

        public ValueTask<ReadOnlyMemory<T>?> PeekAsync(int count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset + count > _buffer.Length)
            {
                return ValueTask.FromResult<ReadOnlyMemory<T>?>(null);
            }
            return ValueTask.FromResult<ReadOnlyMemory<T>?>(_buffer.AsMemory(_offset, count));
        }

        public ValueTask<T?> PeekAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset >= _buffer.Length)
            {
                return ValueTask.FromResult<T?>(null);
            }
            return ValueTask.FromResult<T?>(_buffer[_offset]);
        }
    }
}
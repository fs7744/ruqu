﻿namespace RuQu.Buffer
{
    public interface IChunk<T> : IDisposable
    {
        public ReadOnlySpan<T> UnreadSpan { get; }
        public ReadOnlyMemory<T> UnreadMemory { get; }
        public bool IsEOF { get; }
        public int Length { get; }

        public int Index { get; }

        public T this[int index] { get; }

        public IChunk<T>? Next();

        public void Consume(int count);
    }
}
using System.Runtime.CompilerServices;

namespace RuQu
{
    public ref struct Peeker<T>
    {
        internal readonly ReadOnlySpan<T> span;
        internal int index;

        public readonly int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return span.Length; }
        }

        public Peeker(ReadOnlySpan<T> span)
        {
            this.span = span;
            this.index = 0;
        }

        public readonly T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return span[index];
            }
        }

        public bool TryPeek(out T data)
        {
            var i = index;
            if (i >= span.Length)
            {
                data = default;
                return false;
            }
            data = span[i];
            return true;
        }

        public bool TryPeekOffset(int offset, out T data)
        {
            var i = index + offset;
            if (i >= span.Length)
            {
                data = default;
                return false;
            }
            data = span[i];
            return true;
        }

        public bool TryPeek(int count, out ReadOnlySpan<T> data)
        {
            return TryPeekOffset(0, count, out data);
        }

        public bool TryPeekOffset(int offset, int count, out ReadOnlySpan<T> data)
        {
            var i = index + offset + count;
            if (i > Length)
            {
                data = null;
                return false;
            }
            data = span.Slice(index + offset, count);
            return true;
        }

        public void Read()
        {
            Read(1);
        }

        public void Read(int count)
        {
            index = Math.Min(index + count, Length);
        }
    }
}
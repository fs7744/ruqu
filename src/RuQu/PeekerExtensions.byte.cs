using System.Runtime.CompilerServices;

namespace RuQu
{
    public class BytePeeker : IPeeker<byte>
    {
        internal readonly byte[] bytes;
        internal int index;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return bytes.Length; }
        }

        public BytePeeker(byte[] data)
        {
            this.bytes = data;
            this.index = 0;
        }

        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return bytes[index];
            }
        }

        public bool TryPeekOffset(int offset, out byte data)
        {
            var i = index + offset;
            if (i >= bytes.Length)
            {
                data = default;
                return false;
            }
            data = bytes[i];
            return true;
        }

        public bool TryPeekOffset(int offset, int count, out ReadOnlySpan<byte> data)
        {
            var i = index + offset + count;
            if (i > Length)
            {
                data = null;
                return false;
            }
            data = bytes.AsSpan().Slice(index + offset, count);
            return true;
        }

        public void Read(int count)
        {
            index = Math.Min(index + count, Length);
        }
    }

    public static unsafe partial class Bytes
    {
        public static BytePeeker AsBytePeeker(this byte[] bytes) => new BytePeeker(bytes);



        public static bool TakeRemaining(this BytePeeker peeker, out ReadOnlySpan<byte> span)
        {
            int pos = peeker.index;
            if (pos >= peeker.Length)
            {
                span = default;
                return false;
            }

            span = peeker.bytes.AsSpan()[pos..];
            peeker.index = peeker.Length;
            return true;
        }
    }
}
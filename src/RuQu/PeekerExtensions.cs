namespace RuQu
{
    public static unsafe partial class PeekerExtensions
    {
        public static bool Is<T>(this ref Peeker<T> peeker, Func<T, bool> predicate, out T c)
        {
            if (peeker.TryPeek(out c) && predicate(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool Is<T>(this ref Peeker<T> peeker, delegate*<T, bool> predicate, out T c)
        {
            if (peeker.TryPeek(out c) && predicate(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool IsAny<T>(ref Peeker<T> peeker, out T c)
        {
            if (peeker.TryPeek(out c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool Take<T>(this ref Peeker<T> peeker, delegate*<ReadOnlySpan<T>, int> predicate, out ReadOnlySpan<T> span)
        {
            int pos = peeker.index;
            if ((uint)pos >= (uint)peeker.Length)
            {
                span = default;
                return false;
            }

            ReadOnlySpan<T> remaining = peeker.span[pos..];
            int foundLineLength = predicate(remaining);
            if (foundLineLength >= 0)
            {
                span = remaining[0..foundLineLength];
                peeker.index = pos + foundLineLength + 1;
                return true;
            }
            else
            {
                span = default;
                return false;
            }
        }

        public static bool Take<T>(this ref Peeker<T> peeker, delegate*<T, bool> predicate, out ReadOnlySpan<T> span)
        {
            var count = 0;
            while (peeker.TryPeekOffset(count, out var t) && predicate(t))
            {
                count++;
            }

            if (count > 0 && peeker.TryPeek(count, out span))
            {
                peeker.Read(count);
                return true;
            }
            span = default;
            return false;
        }

        public static bool Take<T>(this ref Peeker<T> peeker, Func<T, bool> predicate, out ReadOnlySpan<T> span)
        {
            var count = 0;
            while (peeker.TryPeekOffset(count, out var t) && predicate(t))
            {
                count++;
            }

            if (count > 0 && peeker.TryPeek(count, out span))
            {
                peeker.Read(count);
                return true;
            }
            span = default;
            return false;
        }
    }
}
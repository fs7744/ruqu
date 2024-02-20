namespace RuQu
{
    public static unsafe partial class PeekerExtensions
    {
        public static bool Is<T>(this ref Peeker<T> peeker, T t) where T : IEquatable<T>?
        {
            if (peeker.TryPeek(out var c) && c.Equals(t))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool IsNot<T>(this ref Peeker<T> peeker, T t, out T c) where T : IEquatable<T>?
        {
            if (peeker.TryPeek(out c) && !c.Equals(t))
            {
                return true;
            }
            return false;
        }

        public static bool IsIn<T>(this ref Peeker<T> peeker, ReadOnlySpan<T> t, out T c) where T : IEquatable<T>?
        {
            if (peeker.TryPeek(out c) && t.Contains(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

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

        public static bool StartsWith<T>(this ref Peeker<T> peeker, ReadOnlySpan<T> value) where T : IEquatable<T>?
        {
            int pos = peeker.index;
            if (pos >= peeker.Length || pos + value.Length >= peeker.Length)
            {
                return false;
            }
            ReadOnlySpan<T> remaining = peeker.span[pos..];
            if (remaining.StartsWith(value))
            {
                peeker.Read(value.Length);
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

        public static bool TakeRemaining<T>(this ref Peeker<T> peeker, out ReadOnlySpan<T> span)
        {
            int pos = peeker.index;
            if (pos >= peeker.Length)
            {
                span = default;
                return false;
            }

            span = peeker.span[pos..];
            peeker.index = peeker.Length;
            return true;
        }
    }
}
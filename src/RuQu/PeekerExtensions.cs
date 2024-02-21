namespace RuQu
{
    public static unsafe partial class PeekerExtensions
    {

        public static bool TryPeek<T>(this IPeeker<T> peeker, out T data)
        {
            return peeker.TryPeekOffset(0, out data);
        }

        public static bool TryPeek<T>(this IPeeker<T> peeker, int count, out ReadOnlySpan<T> data)
        {
            return peeker.TryPeekOffset(0, count, out data);
        }

        public static void Read<T>(this IPeeker<T> peeker)
        {
            peeker.Read(1);
        }

        public static bool Is<T>(this IPeeker<T> peeker, T t) where T : IEquatable<T>?
        {
            if (peeker.TryPeek(out var c) && c.Equals(t))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool IsNot<T>(this IPeeker<T> peeker, T t, out T c) where T : IEquatable<T>?
        {
            if (peeker.TryPeek(out c) && !c.Equals(t))
            {
                return true;
            }
            return false;
        }

        public static bool IsIn<T>(this IPeeker<T> peeker, ReadOnlySpan<T> t, out T c) where T : IEquatable<T>?
        {
            if (peeker.TryPeek(out c) && t.Contains(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool Is<T>(this IPeeker<T> peeker, Func<T, bool> predicate, out T c)
        {
            if (peeker.TryPeek(out c) && predicate(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool Is<T>(this IPeeker<T> peeker, delegate*<T, bool> predicate, out T c)
        {
            if (peeker.TryPeek(out c) && predicate(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool IsAny<T>(IPeeker<T> peeker, out T c)
        {
            if (peeker.TryPeek(out c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool Take<T>(this IPeeker<T> peeker, delegate*<T, bool> predicate, out ReadOnlySpan<T> span)
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

        public static bool Take<T>(this IPeeker<T> peeker, Func<T, bool> predicate, out ReadOnlySpan<T> span)
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
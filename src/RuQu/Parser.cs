namespace RuQu
{
    public delegate int Is<T, R>(IPeeker<T> input, int offset, out R t);

    public delegate bool Delimited<T, X, Y, Z>(IPeeker<T> input, out X x, out Y y, out Z z);

    public static partial class Parser
    {
        public static Func<IPeeker<T>, R> Once<T, R>(this Is<T, R> predicate, string ex) => Once(predicate, i => new FormatException(ex));

        public static Func<IPeeker<T>, R> Once<T, R>(this Is<T, R> predicate, Func<IPeeker<T>, Exception> ex) => i =>
        {
            var c = predicate(i, 1, out var t);
            if (c <= 0)
            {
                throw ex(i);
            }
            i.Read(c);
            return t;
        };

        public static Func<IPeeker<T>, R> Opt<T, R>(this Is<T, R> predicate) => i =>
        {
            var c = predicate(i, 1, out var t);
            if (c > 0)
            {
                i.Read(c);
            }
            return t;
        };

        public static Action<IPeeker<T>> NoMore<T, R>(this Is<T, R> predicate, string ex) => NoMore(predicate, i => new FormatException(ex));

        public static Action<IPeeker<T>> NoMore<T, R>(this Is<T, R> predicate, Func<IPeeker<T>, Exception> ex) => i =>
        {
            var c = predicate(i, 1, out var t);
            if (c > 0)
            {
                throw ex(i);
            }
        };

        public static Is<T, T> Is<T>(Func<T, bool> predicate) => (IPeeker<T> i, int offset, out T t) =>
        {
            if (i.TryPeekOffset(offset, out t) && predicate(t))
            {
                return 1;
            }
            return 0;
        };

        private static IEnumerable<R> _Repeat<T, R>(IPeeker<T> i, Is<T, R> predicate)
        {
            var count = predicate(i, 1, out var t);
            while (count > 0)
            {
                yield return t;
                i.Read(count);
                count = predicate(i, 1, out t);
            }
        }

        public static Func<IPeeker<T>, IEnumerable<R>> Repeat<T, R>(this Is<T, R> predicate) => i => _Repeat(i, predicate);

        private static IEnumerable<R> _Repeat<T, R>(IPeeker<T> i, Is<T, R> predicate, int maxCount)
        {
            for (int j = 0; j < maxCount; j++)
            {
                var count = predicate(i, 1, out var t);
                if (count > 0)
                {
                    yield return t;
                    i.Read(count);
                }
            }
        }

        public static Func<IPeeker<T>, R[]> Repeat<T, R>(this Is<T, R> predicate, int maxCount, Func<IPeeker<T>, Exception> ex) => i =>
        {
            var array = _Repeat(i, predicate, maxCount).ToArray();
            if (array.Length != maxCount)
            {
                throw ex(i);
            }
            return array;
        };

        public static Func<IPeeker<T>, R[]> Repeat<T, R>(this Is<T, R> predicate, int maxCount, string ex) => Repeat(predicate, maxCount, i => new FormatException(ex));

        public static Func<IPeeker<T>, int> Count<T, R>(this Func<IPeeker<T>, IEnumerable<R>> predicate) => i => predicate(i).Count();

        public static Func<IPeeker<T>, IPeekSlice<T>> Take<T>(int count, Func<IPeeker<T>, Exception> ex) => i =>
        {
            if (!i.TryPeek(count, out var d))
            {
                throw ex(i);
            }
            i.Read(count);
            return d;
        };

        public static Func<IPeeker<T>, IPeekSlice<T>> Take<T>(int count, string ex) => Take<T>(count, i => new FormatException(ex));

        public static Func<T, R> Map<T, R0, R>(this Func<T, R0> predicate, Func<R0, R> map) => i => map(predicate(i));

        public static Delimited<T, X, Y, Z> Delimited<T, X, Y, Z>(this Is<T, X> x, Func<IPeeker<T>, Y> y, Is<T, Z> z, string ex) => Delimited(x, y, z, (i) => new FormatException(ex));

        public static Delimited<T, X, Y, Z> Delimited<T, X, Y, Z>(this Is<T, X> x, Func<IPeeker<T>, Y> y, Is<T, Z> z, Func<IPeeker<T>, Exception> ex) => (IPeeker<T> i, out X xo, out Y yo, out Z zo) =>
        {
            var count = x(i, 1, out xo);
            if (count > 0)
            {
                i.Read(count);
                yo = y(i);
                var ii = z(i, 1, out zo);
                if (ii <= 0)
                {
                    ex(i);
                }
                i.Read(ii);
                return true;
            }
            yo = default;
            zo = default;
            return false;
        };

        public static Func<IPeeker<T>, R> Map<T, X, Y, Z, R>(this Delimited<T, X, Y, Z> delimited, Func<bool, X, Y, Z, R> map) => i => 
        {
            var c = delimited(i, out var x, out var y, out var z);
            return map(c, x, y, z);
        };

        public static Is<T, IPeekSlice<T>> ToSlice<T>(this Is<T, T> predicate) => (IPeeker<T> i, int offset, out IPeekSlice<T> t) =>
        {
            var count = 0;
            var o = offset;
            var c = predicate(i, o, out var _);
            while (c > 0) 
            {
                count += c;
                o += c;
                c = predicate(i, o, out var _);
            }

            if (i.TryPeekOffset(offset, count, out t))
            {
                return count;
            }
            t = null;
            return 0;
        };
    }
}
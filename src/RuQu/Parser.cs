namespace RuQu
{
    public delegate int Is<T>(IPeeker<T> input, out T t);

    public static partial class Parser
    {
        public static Func<IPeeker<T>, T> Once<T>(this Is<T> predicate, string ex) => Once(predicate, i => new FormatException(ex));

        public static Func<IPeeker<T>, T> Once<T>(this Is<T> predicate, Func<IPeeker<T>, Exception> ex) => i =>
        {
            var c = predicate(i, out var t);
            if (c <= 0)
            {
                throw ex(i);
            }
            i.Read(c);
            return t;
        };

        public static Is<T> Is<T>(Func<T, bool> predicate, int count) => (IPeeker<T> i, out T t) =>
        {
            if (i.TryPeek(out t) && predicate(t))
            {
                return count;
            }
            return 0;
        };

        private static IEnumerable<T> _Repeat<T>(IPeeker<T> i, Is<T> predicate)
        {
            var count = predicate(i, out var t);
            while (count > 0)
            {
                yield return t;
                i.Read(count);
                count = predicate(i, out t);
            }
        }

        public static Func<IPeeker<T>, IEnumerable<T>> Repeat<T>(this Is<T> predicate) => i => _Repeat(i, predicate);

        private static IEnumerable<T> _Repeat<T>(IPeeker<T> i, Is<T> predicate, int maxCount)
        {
            for (int j = 0; j < maxCount; j++)
            {
                var count = predicate(i, out var t);
                if (count > 0)
                {
                    yield return t;
                    i.Read(count);
                }
            }
        }

        public static Func<IPeeker<T>, T[]> Repeat<T>(this Is<T> predicate, int maxCount, Func<IPeeker<T>, Exception> ex) => i =>
        {
            var array = _Repeat(i, predicate, maxCount).ToArray();
            if (array.Length != maxCount)
            {
                throw ex(i);
            }
            return array;
        };

        public static Func<IPeeker<T>, T[]> Repeat<T>(this Is<T> predicate, int maxCount, string ex) => Repeat(predicate, maxCount, i => new FormatException(ex));

        public static Func<IPeeker<T>, int> Count<T>(this Func<IPeeker<T>, IEnumerable<T>> predicate) => i => predicate(i).Count();
    }
}
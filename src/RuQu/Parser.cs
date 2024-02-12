namespace RuQu
{
    public delegate bool Predicate<T>(T input);

    public static partial class Parser
    {
        public static Predicate<T> Riquired<T>(this Predicate<T> predicate, Func<T, Exception> ex) => i =>
        {
            if (!predicate(i))
            {
                throw ex(i);
            }
            return true;
        };

        public static Predicate<T> Riquired<T>(this Predicate<T> predicate, string ex) => Riquired(predicate, i => new FormatException(ex));

        public static Predicate<IInput<T>> RiquiredNext<T>(this Predicate<IInput<T>> predicate, Func<IInput<T>, Exception> ex) => i =>
        {
            if (predicate(i) && !i.MoveNext())
            {
                throw ex(i);
            }
            return true;
        };

        public static Predicate<IInput<T>> RiquiredNext<T>(this Predicate<IInput<T>> predicate, string ex) => RiquiredNext(predicate, i => new FormatException(ex));

        public static Predicate<IInput<T>> Repeat<T>(this Predicate<IInput<T>> predicate, int count, Func<IInput<T>, Exception> ex) => i =>
        {
            for (var j = 0; j < count; j++)
            {
                if (!predicate(i) || !i.MoveNext())
                {
                    throw ex(i);
                }
            }
            return true;
        };

        public static Predicate<IInput<T>> Repeat<T>(this Predicate<IInput<T>> predicate, int count, string ex) => Repeat<T>(predicate, count, i => new FormatException(ex));

        private static IEnumerable<T> _RepeatUntilNot<T>(IInput<T> i, Predicate<IInput<T>> predicate)
        {
            while (!i.IsEof && predicate(i))
            {
                yield return i.Current;
                if (!i.MoveNext())
                {
                    break;
                }
            }
        }

        public static Func<IInput<T>, IEnumerable<T>> RepeatUntilNot<T>(this Predicate<IInput<T>> predicate) => i => _RepeatUntilNot(i, predicate);

        public static bool Ingore<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() > 0;
        }

        public static Predicate<IInput<T>> Ingore<T>(this Func<IInput<T>, IEnumerable<T>> predicate) => i => predicate(i).Ingore();

        //public static Predicate<IInput<T>> IngoreUntilNot<T>(this Predicate<IInput<T>> predicate)
        //{
        //    var f = RepeatUntilNot(predicate);
        //    return i => f(i).Ingore();
        //}

        public static Predicate<IInput<T>> Then<T>(this Predicate<IInput<T>> predicate, Predicate<IInput<T>> map) => i =>
        {
            return predicate(i) ? map(i) : false;
        };

        private static IEnumerable<T> _RepeatUntil<T>(IInput<T> i, Predicate<IInput<T>> predicate, Predicate<IInput<T>> not)
        {
            while (!i.IsEof && predicate(i) && !not(i))
            {
                yield return i.Current;
                if (!i.MoveNext())
                {
                    break;
                }
            }
        }

        public static Func<IInput<T>, IEnumerable<T>> RepeatUntil<T>(this Predicate<IInput<T>> predicate, Predicate<IInput<T>> not) => i => _RepeatUntil(i, predicate, not);

        //public static Predicate<IInput<T>> IngoreUntil<T>(this Predicate<IInput<T>> predicate, Predicate<IInput<T>> not)
        //{
        //    var f = RepeatUntil(predicate, not);
        //    return i => f(i).Ingore();
        //}

        public static Func<IInput<T>, IEnumerable<T>> Delimited<T>(this Predicate<IInput<T>> prefix, Predicate<IInput<T>> suffix)
        {
            return null;
        }
    }
}
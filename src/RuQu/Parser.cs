namespace RuQu
{
    public delegate bool Predicate<T>(T input);

    public static partial class Parser
    {
        public static Predicate<T> Riquired<T>(this Predicate<T> tag, Func<T, Exception> ex) => i =>
        {
            if (!tag(i))
            {
                throw ex(i);
            }
            return true;
        };

        public static Predicate<T> Riquired<T>(this Predicate<T> tag, string ex) => Riquired(tag, i => new FormatException(ex));

        public static Predicate<IInput<T>> RiquiredNext<T>(this Predicate<IInput<T>> tag, Func<IInput<T>, Exception> ex)  => i =>
        {
            if (tag(i) && !i.MoveNext())
            {
                throw ex(i);
            }
            return true;
        };

        public static Predicate<IInput<T>> RiquiredNext<T>(this Predicate<IInput<T>> tag, string ex) => RiquiredNext(tag, i => new FormatException(ex));
    }
}
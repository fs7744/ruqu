namespace RuQu
{
    public delegate int Tag<T>(T input) where T : IInput;

    public static partial class Tag
    {
        public static Tag<T> Riquired<T>(this Tag<T> tag, Func<T, Exception> ex) where T : IInput => i =>
        {
            var count = tag(i);
            if (count <= 0)
            {
                throw ex(i);
            }
            return count;
        };

        public static Tag<T> Riquired<T>(this Tag<T> tag, string ex) where T : IInput => Riquired(tag, i => new FormatException(ex));

        public static Tag<T> RiquiredNext<T>(this Tag<T> tag, Func<T, Exception> ex) where T : IInput => i =>
        {
            var count = tag(i);
            if (!i.MoveNext())
            {
                throw ex(i);
            }
            return count;
        };

        public static Tag<T> RiquiredNext<T>(this Tag<T> tag, string ex) where T : IInput => RiquiredNext(tag, i => new FormatException(ex));
    }
}
namespace RuQu
{
    public static partial class Charss
    {
        public static Peeker<char> AsPeeker(this string str) => new(str.AsSpan());
    }
}

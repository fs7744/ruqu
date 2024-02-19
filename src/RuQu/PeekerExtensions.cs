namespace RuQu
{
    public static class StringPeekerStructExtensions
    {
        public static Peeker<char> AsPeeker(this string str) => new(str.AsSpan());
    }
}

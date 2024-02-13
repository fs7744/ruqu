namespace RuQu
{
    public static partial class Chars
    {
        public static readonly Is<char> Any = Parser.Is<char>(i => true, 1);
        public static readonly Is<char> IsWhiteSpace = Parser.Is<char>(char.IsWhiteSpace, 1);
        public static readonly Func<IPeeker<char>, int> IngoreWhiteSpace = IsWhiteSpace.Repeat().Count();
        public static readonly Is<char> IsAsciiHexDigit = Parser.Is<char>(char.IsAsciiHexDigit, 1);

        public static Is<char> Is(char c) => Parser.Is<char>(i => i == c, 1);

        public static Is<char> In(char[] arrary) => Parser.Is<char>(i => arrary.Contains(i), 1);

        public static Is<char> In(string str) => Parser.Is<char>(i => str.IndexOf(i) > -1, 1);
    }
}
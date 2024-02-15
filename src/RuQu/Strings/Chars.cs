namespace RuQu
{
    public static partial class Chars
    {
        public static readonly Is<char, char> Any = Parser.Is<char>(i => true);
        public static readonly Is<char, char> IsWhiteSpace = Parser.Is<char>(char.IsWhiteSpace);
        public static readonly Func<IPeeker<char>, int> IngoreWhiteSpace = IsWhiteSpace.Repeat().Count();
        public static readonly Is<char, char> IsAsciiHexDigit = Parser.Is<char>(char.IsAsciiHexDigit);
        public static readonly Is<char, char> IsCR = Parser.Is<char>(i => i == '\r');
        public static readonly Is<char, char> IsLF = Parser.Is<char>(i => i == '\n');
        public static readonly Is<char, char> NotCRLF = Parser.Is<char>(i => i != '\r' && i != '\n');

        public static readonly Is<char, string> IsCRLF = (IPeeker<char> i, int offset, out string str) =>
        {
            if (i.TryPeekOffset(offset, out var c))
            {
                if (c == '\r')
                {
                    if (!i.TryPeekOffset(offset + 1, out var cc))
                    {
                        str = "\r";
                        return 1;
                    }

                    if (cc == '\n')
                    {
                        str = "\r\n";
                        return 2;
                    }
                    else
                    {
                        str = "\r";
                        return 1;
                    }
                }
                else if (c == '\n')
                {
                    str = "\n";
                    return 1;
                }
            }
            str = null;
            return 0;
        };

        public static Is<char, char> Is(char c) => Parser.Is<char>(i => i == c);

        public static Is<char, char> Not(char c) => Parser.Is<char>(i => i != c);

        public static Is<char, char> In(char[] arrary) => Parser.Is<char>(i => arrary.Contains(i));

        public static Is<char, char> In(string str) => Parser.Is<char>(i => str.IndexOf(i) > -1);
    }
}
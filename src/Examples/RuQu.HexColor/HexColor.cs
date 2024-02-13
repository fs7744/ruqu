namespace RuQu
{
    public static class HexColor
    {
        private static readonly Func<IPeeker<char>, char> tag_Start = Chars.Is('#').Once("# is Required.");

        private static Func<IPeeker<char>, string> HexDigit = Parser.Take<char>(6, "Must has 6 AsciiHexDigit").Map(ii =>
        {
            var s = ii.ToString();
            for (var i = 0; i < s.Length; i++)
            {
                if (!char.IsAsciiHexDigit(s[i]))
                {
                    throw new FormatException("Must has 6 AsciiHexDigit");
                }
            }
            return s;
        });

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            var input = Input.From(str);
            tag_Start(input);
            var s = HexDigit(input);
            var r = (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
            if (input.TryPeek(out var c))
            {
                throw new FormatException(c.ToString());
            }
            return r;
        }
    }
}
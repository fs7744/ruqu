namespace RuQu
{
    public static class HexColor
    {
        private static readonly Func<IPeeker<char>, char> tag_Start = Chars.Is('#').Once("# is Required.");

        private static Func<IPeeker<char>, char[]> HexDigit = Chars.IsAsciiHexDigit.Repeat(6, "Must has 6 AsciiHexDigit");

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            var input = Input.From(str);
            tag_Start(input);
            var s = new string(HexDigit(input));
            var r = (Convert.ToByte(s[1..2], 16), Convert.ToByte(s[3..4], 16), Convert.ToByte(s[5..6], 16));
            if (input.TryPeek(out var c))
            {
                throw new FormatException(c.ToString());
            }
            return r;
        }
    }
}

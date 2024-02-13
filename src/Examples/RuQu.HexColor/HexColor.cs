namespace RuQu
{
    public static class HexColor
    {
        private static readonly Func<IPeeker<char>, char> TagStart = Chars.Is('#').Once("# is Required.");

        private static readonly Func<IPeeker<char>, string> HexDigitString = Parser.Take<char>(6, "Must has 6 AsciiHexDigit").Map(ii =>
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

        private static readonly Action<IPeeker<char>> NoMore = Chars.Any.NoMore("Only 7 chars");

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            var input = Input.From(str);
            TagStart(input);
            var s = HexDigitString(input);
            NoMore(input);
            return (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
        }
    }
}
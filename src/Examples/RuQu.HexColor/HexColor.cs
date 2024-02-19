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

    public static class HexColorStruct
    {
        private static void TagStart(ref Peeker<char> input)
        {
            if (!input.TryPeek(out var tag) || tag is not '#')
            {
                throw new FormatException("No perfix with #");
            }
            input.Read();
        }

        private static byte HexDigitColor(ref Peeker<char> input)
        {
            if (!input.TryPeek(2, out var str) || !char.IsAsciiHexDigit(str[0]) || !char.IsAsciiHexDigit(str[1]))
            {
                throw new FormatException("One color must be 2 AsciiHexDigit");
            }
            input.Read(2);
            return Convert.ToByte(str.ToString(), 16);
        }

        private static void NoMore(ref Peeker<char> input)
        {
            if (input.TryPeek(out var _))
            {
                throw new FormatException("Only 7 chars");
            }
        }

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            var input = str.AsPeeker();
            TagStart(ref input);
            var r = (HexDigitColor(ref input), HexDigitColor(ref input), HexDigitColor(ref input));
            NoMore(ref input);
            return r;
        }
    }
}
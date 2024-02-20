namespace RuQu
{
    public static class HexColor
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
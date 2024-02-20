namespace RuQu
{
    public static class HexColor
    {
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

        public static (byte red, byte green, byte blue) Parse(ref Peeker<char> input)
        {
            if (!input.Is('#'))
            {
                throw new FormatException("No perfix with #");
            }
            var r = (HexDigitColor(ref input), HexDigitColor(ref input), HexDigitColor(ref input));
            NoMore(ref input);
            return r;
        }

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            var input = str.AsCharPeeker();
            return Parse(ref input);
        }

        public static (byte red, byte green, byte blue) Parse(byte[] bytes, System.Text.Encoding encoding)
        {
            var input = bytes.AsCharPeeker(encoding);
            return Parse(ref input);
        }
    }
}
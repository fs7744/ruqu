using System.Text;

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

        private static void NoMore<T>(ref Peeker<T> input)
        {
            if (input.TryPeek(out var _))
            {
                throw new FormatException("Only 7 chars");
            }
        }

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            var input = str.AsCharPeeker();
            if (!input.Is('#'))
            {
                throw new FormatException("No perfix with #");
            }
            var r = (HexDigitColor(ref input), HexDigitColor(ref input), HexDigitColor(ref input));
            NoMore(ref input);
            return r;
        }

        private static readonly byte UTF8TagStart = Encoding.UTF8.GetBytes("#")[0];

        public static (byte red, byte green, byte blue) ParseUTF8(byte[] bytes)
        {
            var input = bytes.AsBytePeeker();
            if (!input.Is(UTF8TagStart))
            {
                throw new FormatException("No perfix with #");
            }
            if (!input.TakeRemaining(out var remaining))
            {
                throw new FormatException("Only 7 chars");
            }

            var ci = Encoding.UTF8.GetString(remaining).AsCharPeeker();
            var r = (HexDigitColor(ref ci), HexDigitColor(ref ci), HexDigitColor(ref ci));
            NoMore(ref input);
            return r;
        }
    }
}
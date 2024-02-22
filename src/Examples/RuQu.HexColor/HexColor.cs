using System.Text;

namespace RuQu
{
    public static class HexColor
    {
        private static byte HexDigitColor(IPeeker<char> input)
        {
            if (!input.TryPeek(2, out var str) || !char.IsAsciiHexDigit(str[0]) || !char.IsAsciiHexDigit(str[1]))
            {
                throw new FormatException("One color must be 2 AsciiHexDigit");
            }
            input.Read(2);
            return Convert.ToByte(str.ToString(), 16);
        }

        private static void NoMore<T>(IPeeker<T> input)
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
            var r = (HexDigitColor(input), HexDigitColor(input), HexDigitColor(input));
            NoMore(input);
            return r;
        }

        private static readonly byte UTF8TagStart = Encoding.UTF8.GetBytes("#")[0];

        private static readonly HexColorStreamParser instance = new HexColorStreamParser();


        public static (byte red, byte green, byte blue) ParseStream(Stream stream)
        {
            return instance.Read(stream, new CodeTemplate.SimpleReadOptions() { BufferSize = 8 });
        }

        private static readonly HexColorCharParser _instance = new HexColorCharParser();

        public static (byte red, byte green, byte blue) Parse2(string str)
        {
            return _instance.Read(str, new CodeTemplate.SimpleReadOptions() { BufferSize = 8 });
        }

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
            var r = (HexDigitColor(ci), HexDigitColor(ci), HexDigitColor(ci));
            NoMore(input);
            return r;
        }
    }
}
using Sprache;

namespace RuQu.Benchmark
{
    public static class SpracheHexColorTest
    {
        private static Sprache.Parser<(byte red, byte green, byte blue)> identifier;

        static SpracheHexColorTest()
        {
            identifier =
            from leading in Sprache.Parse.Char('#').Once()
            from red in Sprache.Parse.LetterOrDigit.Repeat(2).Text()
            from green in Sprache.Parse.LetterOrDigit.Repeat(2).Text()
            from blue in Sprache.Parse.LetterOrDigit.Repeat(2).Text()
            select (Convert.ToByte(red, 16), Convert.ToByte(green, 16), Convert.ToByte(blue, 16));
        }

        public static (byte red, byte green, byte blue) Parse(string content) => identifier.Parse(content);
    }
}
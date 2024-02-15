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
            from s in Sprache.Parse.LetterOrDigit.Repeat(6).Text()
            select (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
        }

        public static (byte red, byte green, byte blue) Parse(string content) => identifier.Parse(content);
    }
}
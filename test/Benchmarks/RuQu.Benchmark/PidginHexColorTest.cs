using Pidgin;

namespace RuQu.Benchmark
{
    public static class PidginHexColorTest
    {
        private static Parser<char, char[]> identifier;

        static PidginHexColorTest()
        {
            identifier =
            from leading in Parser.Char('#').SkipAtLeastOnce()
            from c in Parser.LetterOrDigit.Repeat(6)
            select c.ToArray();
        }

        public static (byte red, byte green, byte blue) Parse(string content)
        {
            var c = identifier.ParseOrThrow(content).AsSpan();
            return (Convert.ToByte(new string(c[0..2]), 16), Convert.ToByte(new string(c[2..4]), 16), Convert.ToByte(new string(c[4..6]), 16));
        }
    }
}
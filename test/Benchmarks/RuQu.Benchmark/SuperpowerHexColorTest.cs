using Superpower;
using Superpower.Parsers;

namespace RuQu.Benchmark
{
    public static class SuperpowerHexColorTest
    {
        private static TextParser<string> identifier;

        static SuperpowerHexColorTest()
        {
            identifier =
            from leading in Character.EqualTo('#').AtLeastOnce()
            from c in Character.HexDigit.Repeat(6)
            select new string(c);
        }

        public static (byte red, byte green, byte blue) Parse(string content)
        {
            var s = identifier.Parse(content);
            return (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
        }
    }
}
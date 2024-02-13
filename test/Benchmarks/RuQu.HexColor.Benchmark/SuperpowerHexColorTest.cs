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
            var c = identifier.Parse(content);
            return (Convert.ToByte(c[0..1], 16), Convert.ToByte(c[2..3], 16), Convert.ToByte(c[4..5], 16));
        }
    }
}
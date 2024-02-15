using BenchmarkDotNet.Attributes;

namespace RuQu.Benchmark
{
    public static class HexColorOnlyChar
    {
        private static readonly Func<IPeeker<char>, char> tag_Start = Chars.Is('#').Once("# is Required.");

        private static Func<IPeeker<char>, char[]> HexDigit = Chars.IsAsciiHexDigit.Repeat(6, "Must has 6 AsciiHexDigit");

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            var input = Input.From(str);
            tag_Start(input);
            var s = new string(HexDigit(input));
            var r = (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
            if (input.TryPeek(out var c))
            {
                throw new FormatException(c.ToString());
            }
            return r;
        }
    }

    [MemoryDiagnoser]
    public class HexColorTest
    {
        private static (byte red, byte green, byte blue) HexColorHande(string str)
        {
            if (str.Length == 7 && str[0] is '#')
            {
                for (var i = 1; i < str.Length; i++)
                {
                    if (!char.IsAsciiHexDigit(str[i]))
                    {
                        throw new FormatException("Must has 6 AsciiHexDigit");
                    }
                }
                return (Convert.ToByte(str[1..3], 16), Convert.ToByte(str[3..5], 16), Convert.ToByte(str[5..7], 16));
            }
            throw new ArgumentException("No perfix with #");
        }

        [Benchmark]
        public void Hande_HexColor()
        {
            (byte red, byte green, byte blue) = HexColorHande("#2F14DF");
        }

        [Benchmark]
        public void RuQu_HexColor()
        {
            (byte red, byte green, byte blue) = HexColor.Parse("#2F14DF");
        }

        [Benchmark]
        public void RuQu_HexColorOnlyChar()
        {
            (byte red, byte green, byte blue) = HexColorOnlyChar.Parse("#2F14DF");
        }

        [Benchmark]
        public void Superpower_HexColor()
        {
            (byte red, byte green, byte blue) = SuperpowerHexColorTest.Parse("#2F14DF");
        }

        [Benchmark]
        public void Sprache_HexColor()
        {
            (byte red, byte green, byte blue) = SpracheHexColorTest.Parse("#2F14DF");
        }
    }
}
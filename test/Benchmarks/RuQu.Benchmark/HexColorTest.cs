using BenchmarkDotNet.Attributes;
using System.Text;

namespace RuQu.Benchmark
{

    [MemoryDiagnoser]
    public class HexColorTest
    {
        public byte[] UTF8Bytes { get; }

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

        public HexColorTest()
        {
            UTF8Bytes = Encoding.UTF8.GetBytes("#2F14DF");
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
        public void RuQu_HexColor_Stream()
        {
            (byte red, byte green, byte blue) = HexColor.ParseStream(new MemoryStream(UTF8Bytes));
        }

        [Benchmark]
        public void Superpower_HexColor()
        {
            (byte red, byte green, byte blue) = SuperpowerHexColorTest.Parse("#2F14DF");
        }

        [Benchmark]
        public void Pidgin_HexColor()
        {
            (byte red, byte green, byte blue) = PidginHexColorTest.Parse("#2F14DF");
        }

        [Benchmark]
        public void Sprache_HexColor()
        {
            (byte red, byte green, byte blue) = SpracheHexColorTest.Parse("#2F14DF");
        }
    }
}
﻿using BenchmarkDotNet.Attributes;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuQu.Benchmark
{
    [MemoryDiagnoser]
    public class HexColorTest
    {
        private static (byte red, byte green, byte blue) HexColorHande(string str)
        {
            if (str.Length == 7 && str[0] is '#')
            {
                for (var i = 1; i < 7; i++)
                {
                    if (!char.IsAsciiHexDigit(str[i]))
                    {
                        throw new FormatException("Must has 6 AsciiHexDigit");
                    }
                }
                return (Convert.ToByte(str[1..2], 16), Convert.ToByte(str[3..4], 16), Convert.ToByte(str[5..6], 16));
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

        //[Benchmark]
        //public void Sprache_HexColor()
        //{
        //    (byte red, byte green, byte blue) = SpracheHexColorTest.Parse("#2F14DF");
        //}

        //[Benchmark]
        //public void SSuperpower_HexColor()
        //{
        //    (byte red, byte green, byte blue) = SuperpowerHexColorTest.Parse("#2F14DF");
        //}
    }
}
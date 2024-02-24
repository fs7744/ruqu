namespace RuQu.UT
{
    public class InputStringUT
    {
        [Fact]
        public void HexColorTest()
        {
            (byte red, byte green, byte blue) = HexColor.Parse("#2F14DF");
            Assert.Equal(47, red);
            Assert.Equal(20, green);
            Assert.Equal(223, blue);
            var a = HexColor.StreamParser.WriteToBytes((red, green, blue), new SimpleOptions<(byte red, byte green, byte blue)>());
            using Stream stream = new MemoryStream(a);
            stream.Seek(0, SeekOrigin.Begin);
            (red, green, blue) = HexColor.ParseStream(stream);
            Assert.Equal(47, red);
            Assert.Equal(20, green);
            Assert.Equal(223, blue);
            var s = HexColor.CharParser.WriteToString((red, green, blue), new SimpleOptions<(byte red, byte green, byte blue)>());
            Assert.Equal("#2F14DF", s);
        }

        //[Theory]
        //[InlineData("", 0, false)]
        //[InlineData(" ", 1, false)]
        //[InlineData(" \r", 2, false)]
        //[InlineData(" \r\n", 3, false)]
        //[InlineData(" \n", 2, false)]
        //[InlineData("2 ", 0, true)]
        //[InlineData(" 3 ", 1, true)]
        //[InlineData(" \rn ", 2, true)]
        //[InlineData(" \r\n# ", 3, true)]
        //[InlineData(" \n9 ", 2, true)]
        //public void IngoreWhiteSpaceTest(string s, int r, bool isEOF)
        //{
        //    var i = s.AsCharPeeker();
        //    Assert.Equal(r > 0, i.TakeWhiteSpace(out var span));
        //    Assert.Equal(r, span.Length);
        //    Assert.Equal(isEOF, i.TryPeek(out var c));
        //}

        [Fact]
        public unsafe void INIParseTest()
        {
            var s = """



                [package]
                name="test"
                version="1.1.2"
                  ;sddd
                 [package2]
                name = "test2"
                version = "1.1.2"

                """;

            var a = IniParser.Instance.Read(s, new IniParserOptions());
            Assert.Equal(2, a.Count);
            Assert.Equal(4, a.Values.SelectMany(i => i.Values).Count());
            var ss = IniParser.Instance.WriteToString(a, new IniParserOptions());
        }
    }
}
using RuQu.Csv;

namespace RuQu.UT
{
    public class InputStringUT
    {
        [Fact]
        public void HexColorTest()
        {
            (byte red, byte green, byte blue) = HexColor.CharParser.Read("#2F14DF");
            Assert.Equal(47, red);
            Assert.Equal(20, green);
            Assert.Equal(223, blue);
            var a = HexColor.StreamParser.WriteToBytes((red, green, blue));
            using Stream stream = new MemoryStream(a);
            stream.Seek(0, SeekOrigin.Begin);
            (red, green, blue) = HexColor.StreamParser.Read(stream);
            Assert.Equal(47, red);
            Assert.Equal(20, green);
            Assert.Equal(223, blue);
            var s = HexColor.CharParser.WriteToString((red, green, blue));
            Assert.Equal("#2F14DF", s);
            (red, green, blue) = HexColor.CharParser.Read("#2F15DF".AsSpan());
            Assert.Equal(47, red);
            Assert.Equal(21, green);
            Assert.Equal(223, blue);
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

            var a = IniParser.Instance.Read(s);
            Assert.Equal(2, a.Count);
            Assert.Equal(4, a.Values.SelectMany(i => i.Values).Count());
            var ss = IniParser.Instance.WriteToString(a);
        }

        [Fact]
        public unsafe void CSVParseTest()
        {
            var s = """
                a,b
                1,2
                3sss,3333
                """;

            using var reader = new CsvReader(new StringReader(s), fristIsHeader: true);
            var d = reader.ToArray();
            Assert.Equal(2, reader.Header.Length);
            Assert.Equal(2, d.Length);
            Assert.Equal(4, d.SelectMany(i => i).Count());
        }
    }
}
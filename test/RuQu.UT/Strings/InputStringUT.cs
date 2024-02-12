namespace RuQu.UT
{
    public class InputStringUT
    {
        [Fact]
        public void HexColorTest()
        {
            (byte red, byte green, byte blue) = HexColor.Convert("#2F14DF");
            Assert.Equal(47, red);
            Assert.Equal(20, green);
            Assert.Equal(223, blue);
        }

        [Theory]
        [InlineData("", false, true)]
        [InlineData(" ", true, true)]
        [InlineData(" \r", true, true)]
        [InlineData(" \r\n", true, true)]
        [InlineData(" \n", true, true)]
        [InlineData("2 ", false, false)]
        [InlineData(" 3 ", true, false)]
        [InlineData(" \rn ", true, false)]
        [InlineData(" \r\n# ", true, false)]
        [InlineData(" \n9 ", true, false)]
        public void IngoreWhiteSpaceTest(string s , bool r, bool isEOF)
        {
            var i = Input.From(s);
            Assert.Equal(r, Chars.IngoreWhiteSpace(i));
            Assert.Equal(isEOF, i.IsEof);
        }
    }
}
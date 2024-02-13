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
        }

        [Theory]
        [InlineData("", 0, false)]
        [InlineData(" ", 1, false)]
        [InlineData(" \r", 2, false)]
        [InlineData(" \r\n", 3, false)]
        [InlineData(" \n", 2, false)]
        [InlineData("2 ", 0, true)]
        [InlineData(" 3 ", 1, true)]
        [InlineData(" \rn ", 2, true)]
        [InlineData(" \r\n# ", 3, true)]
        [InlineData(" \n9 ", 2, true)]
        public void IngoreWhiteSpaceTest(string s, int r, bool isEOF)
        {
            var i = Input.From(s);
            Assert.Equal(r, Chars.IngoreWhiteSpace(i));
            Assert.Equal(isEOF, i.TryPeek(out var c));
        }
    }
}
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
    }
}
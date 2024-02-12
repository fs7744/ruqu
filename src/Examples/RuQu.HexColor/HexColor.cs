namespace RuQu
{
    public static class HexColor
    {
        private static readonly Tag<InputString> tag_Start = Tag.Char('#').Riquired("Must start with #").RiquiredNext("Must has content");

        private static byte HexOneColor(InputString input) => System.Convert.ToByte(input.TakeString(2), 16);

        public static (byte red, byte green, byte blue) Convert(string str)
        {
            var input = Input.From(str);
            tag_Start(input);
            return (HexOneColor(input), HexOneColor(input), HexOneColor(input));
        }
    }
}

namespace RuQu
{
    public static class HexColor
    {
        private static readonly Predicate<IInput<char>> tag_Start = Chars.Is('#').Riquired("Must start with #").RiquiredNext("Must has content");

        private static byte HexOneColor(InputString input) => System.Convert.ToByte(input.TakeString(2), 16);

        public static (byte red, byte green, byte blue) Convert(string str)
        {
            var input = Input.From(str);
            tag_Start(input);
            var r = (HexOneColor(input), HexOneColor(input), HexOneColor(input));
            if (!input.IsEof)
            {
                throw new FormatException(input.Current.ToString());
            }
            return r;
        }
    }
}

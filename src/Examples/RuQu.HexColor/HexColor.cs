namespace RuQu
{
    public static class HexColor
    {
        public static readonly HexColorStreamParser StreamParser = new HexColorStreamParser();
        public static readonly HexColorCharParser CharParser = new HexColorCharParser();

        public static (byte red, byte green, byte blue) ParseStream(Stream stream)
        {
            return StreamParser.Read(stream);
        }

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            return CharParser.Read(str);
        }
    }
}
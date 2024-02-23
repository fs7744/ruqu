namespace RuQu
{
    public static class HexColor
    {
        public static readonly HexColorStreamParser StreamParser = new HexColorStreamParser();
        public static readonly HexColorCharParser CharParser = new HexColorCharParser();

        public static (byte red, byte green, byte blue) ParseStream(Stream stream)
        {
            return StreamParser.Read(stream, new SimpleOptions<(byte red, byte green, byte blue)>() { BufferSize = 8 });
        }

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            return CharParser.Read(str, new SimpleOptions<(byte red, byte green, byte blue)>() { BufferSize = 8 });
        }
    }
}
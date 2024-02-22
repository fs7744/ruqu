namespace RuQu
{
    public static class HexColor
    {
        private static readonly HexColorStreamParser instance = new HexColorStreamParser();
        private static readonly HexColorCharParser _instance = new HexColorCharParser();

        public static (byte red, byte green, byte blue) ParseStream(Stream stream)
        {
            return instance.Read(stream, new SimpleReadOptions() { BufferSize = 8 });
        }

        public static (byte red, byte green, byte blue) Parse(string str)
        {
            return _instance.Read(str, new SimpleReadOptions() { BufferSize = 8 });
        }
    }
}
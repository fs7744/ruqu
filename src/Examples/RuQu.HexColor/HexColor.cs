using RuQu.Reader;

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
            var buffer = new StringReaderBuffer(str);
            buffer.Tag('#');
            var c = buffer.AsciiHexDigit(6).ToString();
            buffer.Eof();
            return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
        }
    }
}
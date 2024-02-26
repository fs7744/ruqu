using RuQu.Reader;

namespace RuQu
{
    public class HexColorCharParser : SimpleCharParserBase<(byte red, byte green, byte blue)>
    {
        public HexColorCharParser()
        {
            BufferSize = 8;
        }

        protected override (byte red, byte green, byte blue) Read(IReaderBuffer<char> buffer)
        {
            buffer.Tag('#');
            var c = buffer.AsciiHexDigit(6).ToString();
            buffer.Eof();
            return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
        }


        private static readonly ReadOnlyMemory<char> Tag = "#".AsMemory();

        protected override IEnumerable<ReadOnlyMemory<char>> ContinueWrite((byte red, byte green, byte blue) value)
        {
            (byte red, byte green, byte blue) = value;
            yield return Tag;
            yield return Convert.ToHexString(new byte[] { red, green, blue }).AsMemory();
        }
    }
}
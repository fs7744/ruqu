using RuQu.Buffer;

namespace RuQu
{
    public class HexColorChunkParser : ChunkCharParserBase<(byte red, byte green, byte blue)>
    {
        protected override (byte red, byte green, byte blue) Read(IChunkReader<char> buffer)
        {
            buffer.Tag('#');
            var c = buffer.AsciiHexDigit(6).ToString();
            return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
        }
    }
}
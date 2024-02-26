using RuQu.Reader;
using System.Text;

namespace RuQu
{
    public class HexColorStreamParser : SimpleStreamParserBase<(byte red, byte green, byte blue)>
    {
        public HexColorStreamParser()
        {
            BufferSize = 8;
        }

        protected override IEnumerable<ReadOnlyMemory<byte>> ContinueWrite((byte red, byte green, byte blue) value)
        {
            (byte red, byte green, byte blue) = value;
            yield return "#"u8.ToArray().AsMemory();
            yield return Encoding.UTF8.GetBytes(Convert.ToHexString(new byte[] { red, green, blue })).AsMemory();
        }

        protected override (byte red, byte green, byte blue) Read(IReaderBuffer<byte> buffer)
        {
            buffer.IngoreUtf8Bom();
            buffer.Tag((byte)'#');
            if (!buffer.Peek(6, out var bytes))
            {
                throw new FormatException("Only 7 utf-8 chars");
            }

            var c = Encoding.UTF8.GetString(bytes);
            buffer.Consume(6);
            buffer.Eof();
            return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
        }
    }
}
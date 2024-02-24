using RuQu.Reader;
using System.Text;

namespace RuQu
{
    public class HexColorStreamParser : SimpleStreamParserBase<(byte red, byte green, byte blue), SimpleOptions<(byte red, byte green, byte blue)>>
    {
        protected override (byte red, byte green, byte blue) ContinueRead(IReadBuffer<byte> buffer, SimpleOptions<(byte red, byte green, byte blue)> options)
        {
            var bytes = buffer.Remaining;
            if (bytes.Length > 7)
            {
                throw new FormatException("Only 7 utf-8 chars");
            }

            if (!buffer.IsFinalBlock && bytes.Length < 7)
            {
                buffer.AdvanceBuffer(0);
                return default;
            }

            if (buffer.IsFinalBlock && bytes.Length < 7)
            {
                throw new FormatException("Must 7 utf-8 chars");
            }

            if (bytes[0] is not (byte)'#')
            {
                throw new FormatException("No perfix with #");
            }

            var c = Encoding.UTF8.GetString(bytes[1..]);

            return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
        }

        protected override void HandleReadFirstBlock(IReadBuffer<byte> buffer)
        {
            buffer.IngoreUtf8Bom();
        }

        protected override IEnumerable<ReadOnlyMemory<byte>> ContinueWrite(SimpleOptions<(byte red, byte green, byte blue)> options)
        {
            (byte red, byte green, byte blue) = options.WriteObject;
            yield return "#"u8.ToArray().AsMemory();
            yield return Encoding.UTF8.GetBytes(Convert.ToHexString(new byte[] { red, green, blue })).AsMemory();
        }
    }
}
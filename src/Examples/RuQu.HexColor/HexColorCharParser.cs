using RuQu.Reader;
using System.Buffers;
using System.Text;

namespace RuQu
{
    public class HexColorCharParser : SimpleCharParserBase<(byte red, byte green, byte blue), SimpleOptions<(byte red, byte green, byte blue)>>
    {
        protected override (byte red, byte green, byte blue) ContinueRead(IReadBuffer<char> buffer, SimpleOptions<(byte red, byte green, byte blue)> options)
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

            if (bytes[0] is not '#')
            {
                throw new FormatException("No perfix with #");
            }

            var c = new string(bytes[1..]);

            return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
        }

        public override bool ContinueWrite(IBufferWriter<char> writer, SimpleOptions<(byte red, byte green, byte blue)> options)
        {
            var span = writer.GetSpan(7);
            span[0] = '#';
            (byte red, byte green, byte blue) = options.WriteObject;
            var str = Convert.ToHexString(new byte[] { red, green, blue });
            str.CopyTo(span.Slice(1));
            writer.Advance(str.Length + 1);
            return true;
        }
    }
}
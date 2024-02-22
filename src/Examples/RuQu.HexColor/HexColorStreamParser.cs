using RuQu.Reader;
using System.Text;

namespace RuQu
{

    public class HexColorStreamParser : SimpleStreamParserBase<(byte red, byte green, byte blue), SimpleReadOptions, IntState>
    {
        protected override (byte red, byte green, byte blue) ContinueRead(ref ByteReadBuffer bufferState, ref IntState state)
        {
            var bytes = bufferState.Remaining;
            if (bytes.Length > 7)
            {
                throw new FormatException("Only 7 utf-8 chars");
            }

            if (!bufferState.IsFinalBlock && bytes.Length < 7)
            {
                bufferState.AdvanceBuffer(0);
                return default;
            }

            if (bufferState.IsFinalBlock && bytes.Length < 7)
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

        protected override void HandleFirstBlock(ref ByteReadBuffer bufferState)
        {
            bufferState.IngoreUtf8Bom();
        }
    }
}